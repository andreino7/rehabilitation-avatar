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
using omicron;
using omicronConnector;

public class OmicronKinectEventClient : OmicronEventClient {
	public OmicronKinectManager kinectManager;

	public int bodyID = -1;

	public GameObject jointPrefab;

	public GameObject handStatePrefab;

	bool jointsInitialized = false;
	GameObject[] joints;

	GameObject leftHandStateMarker;
	GameObject rightHandStateMarker;

	public enum KinectHandState { Unknown, NotTracked, Open, Closed, Lasso };
	int leftHandState;
	int rightHandState;

	public Material[] materials;

	public float timeout = 5;
	float lastUpdateTime;

	// Use this for initialization
	new void Start () {
		InitOmicron ();
		lastUpdateTime = Time.time;
	}

	void InitializeJoints( int jointCount )
	{
		joints = new GameObject[jointCount];
		for (int i = 0; i < jointCount; i++)
		{
			joints[i] = Instantiate(jointPrefab) as GameObject;

			joints[i].transform.parent = transform;
			joints[i].name = "Joint "+i;

			if( i == 9 ) // Left hand
			{
				leftHandStateMarker = Instantiate (handStatePrefab) as GameObject;
				leftHandStateMarker.transform.parent = joints[i].transform;
				leftHandStateMarker.transform.localPosition = Vector3.zero;
			}
			else if( i == 19 ) // Right hand
			{
				rightHandStateMarker = Instantiate (handStatePrefab) as GameObject;
				rightHandStateMarker.transform.parent = joints[i].transform;
				rightHandStateMarker.transform.localPosition = Vector3.zero;
			}
		}
		jointsInitialized = true;
	}

	void RemoveBody()
	{
		kinectManager.RemoveBody(bodyID);
		SetFlaggedForRemoval();
		Destroy( gameObject );
	}

	void SetJointVisible(int jointID, bool value)
	{
		joints[jointID].GetComponent<Renderer>().enabled = value;
	}

	// Update is called once per frame
	void Update () {
		/*
		HandState_Unknown	= 0,
        HandState_NotTracked	= 1,
        HandState_Open	= 2,
        HandState_Closed	= 3,
        HandState_Lasso	= 4
		*/
		if( leftHandStateMarker )
		{
			switch(leftHandState)
			{
			//case(0): leftHandStateMarker.renderer.material = materials [0]; break;
			case(1): leftHandStateMarker.GetComponent<Renderer>().material = materials [1]; break;
			case(2): leftHandStateMarker.GetComponent<Renderer>().material = materials [2]; break;
			case(3): leftHandStateMarker.GetComponent<Renderer>().material = materials [3]; break;
			case(4): leftHandStateMarker.GetComponent<Renderer>().material = materials [4]; break;
			}
		}

		if( rightHandStateMarker )
		{
			switch(rightHandState)
			{
			//case(0): rightHandStateMarker.renderer.material = materials [0]; break;
			case(1): rightHandStateMarker.GetComponent<Renderer>().material = materials [1]; break;
			case(2): rightHandStateMarker.GetComponent<Renderer>().material = materials [2]; break;
			case(3): rightHandStateMarker.GetComponent<Renderer>().material = materials [3]; break;
			case(4): rightHandStateMarker.GetComponent<Renderer>().material = materials [4]; break;
			}
		}

		if( leftHandStateMarker && leftHandStateMarker.GetComponent<KinectHandGrabber>() )
		{
			leftHandStateMarker.GetComponent<KinectHandGrabber>().handState = leftHandState;
		}
		if( rightHandStateMarker && rightHandStateMarker.GetComponent<KinectHandGrabber>() )
		{
			rightHandStateMarker.GetComponent<KinectHandGrabber>().handState = rightHandState;
		}

		if ( Time.time > lastUpdateTime + timeout )
		{
			RemoveBody();
		}
	}
	
	void OnEvent( EventData e )
	{
		if (e.serviceType == EventBase.ServiceType.ServiceTypeMocap )
		{
			int sourceID = (int)e.sourceId;

			if( sourceID != bodyID )
				return;

			lastUpdateTime = Time.time;

			// 27 = OpenNI or Kinect v1.x skeleton; 29 = Kinect v2.0
			// See https://github.com/uic-evl/omicron/wiki/MSKinectService
			// for joint ID names
			int jointCount = (int)e.extraDataItems;

			if( !jointsInitialized )
			{
				InitializeJoints(jointCount);
			}
			for( int i = 0; i < jointCount; i++ )
			{
				float[] posArray = new float[] { 0, 0, 0 };
				e.getExtraDataVector3(i, posArray );

				// 

				joints[i].transform.localPosition = new Vector3( posArray[0], posArray[1], -posArray[2] );

				// Hide unused/inactive joints
				if( posArray[0] == 0 && posArray[1] == 0 && posArray[2] == 0 )
				{
					SetJointVisible(i, false);
				}
				else if( !joints[i].GetComponent<Renderer>().enabled )
				{
					SetJointVisible(i, true);
				}
			}

			// Hand state is encoded using the event's orientation field
			leftHandState = (int)e.orw;
			rightHandState = (int)e.orx;
		}
	}
}
