﻿/**************************************************************************************************
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
	public FlatAvatarController patient;

	public Vector3 kinectSensorPosition;
	public Vector3 kinectSensorTilt;

	public bool enableBodyTracking = true;
	public bool enableSpeechRecognition = true;
	public float minimumSpeechConfidence = 0.3f;

	private Dictionary<int, float> trackedBodies;
	//private List<int> trackedBodies;

	public GameObject[] voiceCommandListeners;

	// Use this for initialization
	new void Start () {
		trackedBodies = new Dictionary<int, float> ();
		InitOmicron ();
	}

	// Update is called once per frame
	void Update () {
	
	}

	void OnEvent( EventData e )
	{
		if (enableBodyTracking && e.serviceType == EventBase.ServiceType.ServiceTypeMocap )
		{
			int sourceID = (int)e.sourceId;
			if(sourceID > 1) {
			//	Debug.Log(sourceID);
				float[] jointPosition = new float[3];
				e.getExtraDataVector3(0, jointPosition);
				if( !trackedBodies.ContainsKey( sourceID ) )
				{
				//	Debug.Log(sourceID);
				//	Debug.Log("new key");
					trackedBodies.Add( sourceID, jointPosition[2]);
					Debug.Log (trackedBodies.Count);
					if (patient.bodyId != -1) {
						CreateBody( sourceID );
					}
				//	Debug.Log (trackedBodies[sourceID]);
				} else {
					trackedBodies[sourceID] = jointPosition[2];
				}
				if(patient.bodyId == -1) {
					UpdatePatientBody();
				}
			}
		}
		else if (enableSpeechRecognition && e.serviceType == EventBase.ServiceType.ServiceTypeSpeech)
		{
			string speechString = e.getExtraDataString();
			float speechConfidence = e.posx;

			//Debug.Log("Received Speech: '" + speechString + "' at " +speechConfidence+ " confidence" );

			if( speechConfidence >= minimumSpeechConfidence )
			{
				foreach( GameObject voiceListeners in voiceCommandListeners )
				{
					voiceListeners.SendMessage("OnVoiceCommand", speechString);
				}
			}
		}
	}

	void CreateBody( int sourceId )
	{
		GameObject body;

		body = Instantiate(therapist) as GameObject;

		body.transform.parent = transform;
		body.layer = gameObject.layer;
		body.GetComponent<FlatAvatarController>().bodyId = sourceId;
		body.GetComponent<FlatAvatarController>().kinectManager = this;
		//trackedBodies.Add( sourceID, body );
	}

	private void UpdatePatientBody () {

		float minZ = 10000;
		//float min2z = 10000;
		int minSourceBody = patient.bodyId;
		//int minSourceBodyT = therapist.GetComponent<FlatAvatarController>().bodyID;

		foreach (int bodyId in trackedBodies.Keys) {
			if ( trackedBodies[bodyId] < minZ && bodyId > 1 ) {
				//min2z = minZ;
				//minSourceBodyT = minSourceBody;
				minZ = trackedBodies[bodyId];
				//Debug.Log (trackedBodies[bodyId]);
				minSourceBody = bodyId;
			}
		}
		//Debug.Log (minZ);
		if(minSourceBody != patient.bodyId) {
			Debug.Log("Patient switched!!! New id: " + minSourceBody + ", z: " + minZ);
		}
		patient.bodyId = minSourceBody;
		//therapist.GetComponent<FlatAvatarController>().bodyID = minSourceBodyT;

		//Debug.Log (FlatAvatarController.patientBodyID);
	}

	public int GetPatientId(){
		return patient.bodyId;
	}

	public void RemoveBody(int bodyId )
	{
		trackedBodies.Remove( bodyId );
	}

}
