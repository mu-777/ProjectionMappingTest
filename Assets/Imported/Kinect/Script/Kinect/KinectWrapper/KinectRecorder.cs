using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Kinect;
using System;

public class KinectRecorder : MonoBehaviour {

    private bool color = true;
    private bool depth = true;
    private bool skelton = false;
    public DeviceOrEmulator devOrEmu;

    private KinectInterface kinect;

    public string outputFile = "Assets/Kinect/Recordings/playback";


    private bool isRecording = false;
    private ArrayList currentSkeltonData = new ArrayList();
    private ArrayList currentDepthData = new ArrayList();
    private ArrayList currentColorData = new ArrayList();

    //add by lxjk
    private int fileCount = 0;
    //end lxjk


    // Use this for initialization
    void Start() {
        kinect = devOrEmu.getKinect();
    }

    // Update is called once per frame
    void Update() {
        if (!isRecording) {
            if (Input.GetKeyDown(KeyCode.F9)) {
                StartRecord();
            }
        } else {
            if (Input.GetKeyDown(KeyCode.F10)) {
                StopRecord();
            }
        }
    }

    void FixedUpdate() {
        if (isRecording) {
            if (skelton && kinect.pollSkeleton()) {
                currentSkeltonData.Add(kinect.getSkeleton());
            }
            if (depth && kinect.pollDepth()) {
                currentDepthData.Add(kinect.getDepth());
            }
            if (color && kinect.pollColor()) {
                currentColorData.Add(kinect.getColor());
            }
        }
    }

    void StartRecord() {
        isRecording = true;
        Debug.Log("start recording");
    }

    void StopRecord() {
        isRecording = false;
        Debug.Log("stop recording");

        //edit by lxjk
        string filePath = outputFile + fileCount.ToString();
        FileStream output = new FileStream(@filePath, FileMode.Create);
        //end lxjk
        BinaryFormatter bf = new BinaryFormatter();
        if (skelton) {
            SerialSkeletonFrame[] data = new SerialSkeletonFrame[currentSkeltonData.Count];
            for (int ii = 0; ii < currentSkeltonData.Count; ii++) {
                data[ii] = new SerialSkeletonFrame((NuiSkeletonFrame)currentSkeltonData[ii]);
            }
            bf.Serialize(output, data);
        }
        if (color || depth) {
            Debug.Log("saving...");
            SerialKinectFrameData[] data = new SerialKinectFrameData[currentDepthData.Count];
            for (int i = 0; i < currentDepthData.Count; i++) {
                data[i] = new SerialKinectFrameData((short[])currentDepthData[i],
                                                    (Color32[])currentColorData[i]);
            }
            bf.Serialize(output, data);
        }

        output.Close();
        fileCount++;
    }

}


