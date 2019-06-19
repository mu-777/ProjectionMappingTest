using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using Kinect;

public class KinectEmulator : MonoBehaviour, KinectInterface {

    private bool color = true;
    private bool depth = true;
    private bool skelton = false;
    public string inputFile = "Assets/Kinect/Recordings/playback0";
    private string inputFileDefault = "Assets/Kinect/Recordings/playbackDefault";
    private float playbackSpeed = 0.0333f;
    private float timer = 0;
    private bool isDefault = true;

    /// <summary>
    /// how high (in meters) off the ground is the sensor
    /// </summary>
    public float sensorHeight;
    /// <summary>
    /// where (relative to the ground directly under the sensor) should the kinect register as 0,0,0
    /// </summary>
    public Vector3 kinectCenter;
    /// <summary>
    /// what point (relative to kinectCenter) should the sensor look at
    /// </summary>
    public Vector4 lookAt;

    /// <summary>
    ///variables used for updating and accessing depth data
    /// </summary>
    private bool newFrame = false;
    private int curFrame = 0;
    private NuiSkeletonFrame[] skeletonFrame;
    private List<short[]> depthFrames;
    private List<Color32[]> colorFrames;



    /// <summary>
    ///variables used for updating and accessing depth data
    /// </summary>
    //private bool updatedColor = false;
    //private bool newColor = false;
    //private Color32[] colorImage;
    /// <summary>
    ///variables used for updating and accessing depth data
    /// </summary>
    //private bool updatedDepth = false;
    //private bool newDepth = false;
    //private short[] depthPlayerData;


    // Use this for initialization
    void Start() {
        LoadPlaybackFile(inputFile);
    }

    void Update() {
        timer += Time.deltaTime;
        if (Input.GetKeyUp(KeyCode.F12)) {
            if (isDefault) {
                isDefault = false;
                LoadPlaybackFile(inputFile);
            } else {
                isDefault = true;
                LoadPlaybackFile(inputFile);
            }
        }
    }

    // Update is called once per frame
    void LateUpdate() {
        newFrame = false;
    }

    void LoadPlaybackFile(string filePath)  {
        FileStream input = new FileStream(@filePath, FileMode.Open);
        BinaryFormatter bf = new BinaryFormatter();
        SerialKinectFrameData[] serialKinectFrame = (SerialKinectFrameData[])bf.Deserialize(input);
        if (depth) {
            depthFrames = new List<short[]>();
        }
        if (color) {
            colorFrames = new List<Color32[]>();
        }
        for (int ii = 0; ii < serialKinectFrame.Length; ii++) {
            if (depth) {
                depthFrames.Add(serialKinectFrame[ii].deserializeDepth());
            }
            if (color) {
                colorFrames.Add(serialKinectFrame[ii].deserializeColor());
            }
        }
        if (skelton) {
            SerialSkeletonFrame[] serialSkeleton = (SerialSkeletonFrame[])bf.Deserialize(input);
            skeletonFrame = new NuiSkeletonFrame[serialSkeleton.Length];
            for (int ii = 0; ii < serialSkeleton.Length; ii++) {
                skeletonFrame[ii] = serialSkeleton[ii].deserialize();
            }
        }
        input.Close();
        timer = 0;
        Debug.Log("Simulating " + @filePath);
    }

    float KinectInterface.getSensorHeight() {
        return sensorHeight;
    }
    Vector3 KinectInterface.getKinectCenter() {
        return kinectCenter;
    }
    Vector4 KinectInterface.getLookAt() {
        return lookAt;
    }

    bool pollFrame() {
        int frame = Mathf.FloorToInt(Time.realtimeSinceStartup / playbackSpeed);
        if (frame > curFrame) {
            curFrame = frame;
            newFrame = true;
            return newFrame;
        }
        return newFrame;
    }

    bool KinectInterface.pollSkeleton() {
        if (skelton) {
            return pollFrame();
        } else {
            return false;
        }
    }

    NuiSkeletonFrame KinectInterface.getSkeleton() {
        if (skelton) {
            return skeletonFrame[curFrame % skeletonFrame.Length];
        } else {
            return new NuiSkeletonFrame();
        }
    }

    //NuiSkeletonBoneOrientation[] KinectInterface.getBoneOrientations(NuiSkeletonFrame skeleton) {
    //    return null;
    //}

    NuiSkeletonBoneOrientation[] KinectInterface.getBoneOrientations(NuiSkeletonData skeletonData) {
        NuiSkeletonBoneOrientation[] boneOrientations = new NuiSkeletonBoneOrientation[(int)(NuiSkeletonPositionIndex.Count)];
        NativeMethods.NuiSkeletonCalculateBoneOrientations(ref skeletonData, boneOrientations);
        return boneOrientations;
    }

    bool KinectInterface.pollColor() {
        if (color) {
            return pollFrame();
        } else {
            return false;
        }
    }

    Color32[] KinectInterface.getColor() {
        if (color) {
            return colorFrames[curFrame % colorFrames.Count];
        } else {
            return null;
        }
    }

    bool KinectInterface.pollDepth() {
        if (depth) {
            return pollFrame();
        } else {
            return false;
        }
    }

    short[] KinectInterface.getDepth() {
        if (depth) {
            return depthFrames[curFrame % depthFrames.Count];
        } else {
            return null;
        }
    }
}
