/**************************************************************************************************
* THE OMICRON PROJECT
*-------------------------------------------------------------------------------------------------
* Copyright 2010-2014             Electronic Visualization Laboratory, University of Illinois at Chicago
* Authors:                                                                                
* Arthur Nishimoto                anishimoto42@gmail.com
*-------------------------------------------------------------------------------------------------
* Copyright (c) 2010-2014, Electronic Visualization Laboratory, University of Illinois at Chicago
* All rights reserved.
* Redistribution and use in source and binary forms, with or without modification, are permitted
* provided that the following conditions are met:
*
* Redistributions of source code must retain the above copyright notice, this list of conditions
* and the following disclaimer. Redistributions in binary form must reproduce the above copyright
* notice, this list of conditions and the following disclaimer in the documentation and/or other
* materials provided with the distribution.
*
* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR
* IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
* FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
* CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
* DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF
* USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
* WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN
* ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*************************************************************************************************/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using omicron;
using omicronConnector;

public class OmicronKinectManager : OmicronEventClient {

	public GameObject therapist;
	public GameObject menuManager;
	public FlatAvatarController patient;

	public Vector3 kinectSensorPosition;
	public Vector3 kinectSensorTilt;

	public bool enableBodyTracking = true;
	public bool enableSpeechRecognition = true;
	public float minimumSpeechConfidence = 0.3f;

	private Dictionary<int, float> trackedBodies;
	private static int voiceListeners = 1;

	public GameObject[] voiceCommandListeners;

	// Use this for initialization
	new void Start () {
		trackedBodies = new Dictionary<int, float> ();
		voiceCommandListeners = new GameObject[voiceListeners];
		voiceCommandListeners [0] = menuManager;
		InitOmicron ();
	}

	void OnEvent( EventData e ) {
		if (enableBodyTracking && e.serviceType == EventBase.ServiceType.ServiceTypeMocap ) {
			int sourceID = (int)e.sourceId;
			if(sourceID > 1) {
				float[] jointPosition = new float[3];
				e.getExtraDataVector3(0, jointPosition);
				if( !trackedBodies.ContainsKey (sourceID) ) {
					trackedBodies.Add( sourceID, jointPosition[2]);
					if (patient.bodyId != -1) {
						CreateBody( sourceID );
					}
				} else {
					trackedBodies[sourceID] = jointPosition[2];
				}
				if(patient.bodyId == -1) {
					UpdatePatientBody();
				}
			}
		} else if (enableSpeechRecognition && e.serviceType == EventBase.ServiceType.ServiceTypeSpeech) {
			string speechString = e.getExtraDataString();
			float speechConfidence = e.posx;

			//Debug.Log("Received Speech: '" + speechString + "' at " +speechConfidence+ " confidence" );

			if( speechConfidence >= minimumSpeechConfidence ) {
				foreach( GameObject voiceListeners in voiceCommandListeners ) {
					voiceListeners.SendMessage("OnVoiceCommand", speechString);
				}
			}
		}
	}

	void CreateBody( int sourceId ) {
		GameObject body;
		body = Instantiate(therapist) as GameObject;
		body.transform.parent = transform;
		body.layer = gameObject.layer;
		body.GetComponent<FlatAvatarController>().bodyId = sourceId;
		body.GetComponent<FlatAvatarController>().kinectManager = this;
	}

	private void UpdatePatientBody () {

		float minZ = int.MaxValue;
		int minSourceBody = patient.bodyId;

		foreach (int bodyId in trackedBodies.Keys) {
			if ( trackedBodies[bodyId] < minZ && bodyId > 1 ) {
				minZ = trackedBodies[bodyId];
				minSourceBody = bodyId;
			}
		}

		if(minSourceBody != patient.bodyId) {
			Debug.Log("Patient switched!!! New id: " + minSourceBody + ", z: " + minZ);
		}
		patient.bodyId = minSourceBody;

	}

	public int GetPatientId() {
		return patient.bodyId;
	}

	public void RemoveBody(int bodyId )
	{
		trackedBodies.Remove( bodyId );
	}

}
