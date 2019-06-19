using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class DepthMeshManager : MonoBehaviour {
    [SerializeField]
    private DepthWrapper _kinectDepthWrapper;
    [SerializeField]
    private KinectSensor _kinectSensor;

    [SerializeField]
    private Material _mat;

    List<GameObject> _meshes = new List<GameObject>();
    private string _meshName = "depthMesh";

    private int _depthHeight = 240;
    private int _depthWidth = 320;

    private static int VertexUpperBound = 65000;
    public float depthNearClipMeter = 0.01f;
    private float depthFarClipMeter = 3f;

    private float _focalLengthX, _focalLengthY;

    void Awake() {
        int subdivHeight = VertexUpperBound / _depthWidth;
        int subdivNum = (int)Math.Ceiling((double)_depthHeight / (double)subdivHeight);

        for (int subdiv = 0; subdiv < subdivNum; subdiv++) {
            var go = new GameObject(_meshName + subdiv);
            go.transform.parent = this.transform;
            go.transform.localPosition = Vector3.zero;
            go.layer = this.gameObject.layer;

            var mesh = createMesh(Mathf.Min(subdivHeight, _depthHeight - subdivHeight * subdiv), _depthWidth,
                                  _depthHeight / 2 - subdivHeight * subdiv, _depthWidth / 2);
            var filter = go.AddComponent<MeshFilter>();
            var renderer = go.AddComponent<MeshRenderer>();
            filter.sharedMesh = mesh;
            renderer.material = _mat;
            _meshes.Add(go);
        }
        //this.transform.Translate(Vector3.forward * 100);
        //this.transform.Rotate(Vector3.up * 180);

        _focalLengthX = 0.5f * _depthWidth / Mathf.Tan(Kinect.Constants.NuiDepthHorizontalFOV * Mathf.Deg2Rad);
        _focalLengthY = 0.5f * _depthHeight / Mathf.Tan(Kinect.Constants.NuiDepthVerticalFOV * Mathf.Deg2Rad);

        this.transform.LookAt(_kinectSensor.lookAt);
    }

    void Start() {

    }

    void Update() {
        if (_kinectDepthWrapper.pollDepth()) {
            applyDepthToMesh(_kinectDepthWrapper.depthImg);
        }
    }

    private void applyDepthToMesh(short[] depthMap) {
        int depthMapIdx = 0;
        foreach (var mesh in _meshes) {
            var filter = mesh.GetComponent<MeshFilter>();
            var vertices = filter.sharedMesh.vertices;
            for (int i = 0; i < vertices.Length; i++) {
                vertices[i] = depth2pos(depthMapIdx, (float)depthMap[depthMapIdx++] * 0.001f);
            }
            filter.sharedMesh.vertices = vertices;
        }
    }

    private Vector3 depth2pos(int idx, float depth) {
        if (depth < depthNearClipMeter) {
            depth = depthFarClipMeter;
        }
        return new Vector3(depth * (_depthWidth / 2 - idx % _depthWidth) / _focalLengthX,
                           depth * (_depthHeight / 2 - idx / _depthWidth) / _focalLengthY,
                           depth);
    }

    private Mesh createMesh(int height, int width, int offsetHeight = 0, int offsetWidth = 0) {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        for (int h = 0; h < height; h++) {
            for (int w = 0; w < width; w++) {
                vertices.Add(new Vector3(-w + offsetWidth,
                                         -h + offsetHeight,
                                         0));
                normals.Add(-Vector3.forward);
                if (h >= height - 1 || w >= width - 1) {
                    continue;
                }
                int i = h * width + w;
                triangles.AddRange(divideSq2Tri(new int[] { i, i + 1, i + width, i + width + 1},
                                                false));
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        return mesh;
    }

    private List<int> divideSq2Tri(int[] square, bool isInv = false) {
        if (!isInv) {
            return new List<int>() {
                square[0], square[2], square[3], square[0], square[3], square[1]
            };
        } else {
            return new List<int>() {
                square[0], square[3], square[2], square[0], square[1], square[3]
            };
        }
    }
}
