using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelaunayDrawer : MonoBehaviour {

	static Vector3 createVector(float x, float y)
	{
		return new Vector3 (x, y, 0);
	}
	static float random(float a, float b)
	{
		return Random.Range (a, b);
	}
	static float sqrt(float v)
	{
		return Mathf.Sqrt (v);
	}

	[System.Serializable]
	public class HSVColor
	{
		public float h;
		public float s;
		public float v;
		public HSVColor(float h_, float s_, float v_){
			h = h_;
			s = s_;
			v = v_;
		}
		public Color GetColor()
		{
			return Color.HSVToRGB (h, s, v);
		}
	}

	DelaunayTriangulation delaunayTriangulation;

	Mesh mesh;

	[SerializeField]
	int num = 100;
	
	[SerializeField]
	Color color;
	[SerializeField]
	HSVColor hsvColor;
	
	[SerializeField]
	Rect rect = new Rect(-5, -5, 10, 10);

	List<Color> colors;

	List<Vector3> movePosList;
	List<Vector3> orgPosList;
	List<Vector3> currentPosList;

	List<HSVColor> hsvColors;

	[SerializeField]
	float ampMoveZ = 1;
	[SerializeField]
	Vector3 speed = Vector3.one;

	void Init()
	{
		delaunayTriangulation = new DelaunayTriangulation();
		Color.RGBToHSV (color, out hsvColor.h, out hsvColor.s, out hsvColor.v);
		
		orgPosList = new List<Vector3> ();
		// randomPoints
		for (var i = 0; i < num; i++) {
			orgPosList.Add(createVector(random(rect.xMin, rect.xMax), random(rect.yMin, rect.yMax)));
		}
		int pointCount = orgPosList.Count;

		// speed
		spList = new List<Vector3> ();
		for (int i = 0; i < pointCount; i++) {
			Vector3 sp = new Vector3 (Random.Range (-1.0f, 1.0f), Random.Range (-1.0f, 1.0f), Random.Range (-1.0f, 1.0f));
			spList.Add (sp);
		}

		// current
		currentPosList = new List<Vector3> ();
		currentPosList.AddRange (orgPosList);
		movePosList = new List<Vector3> ();
		for (int i = 0; i < pointCount; i++) {
			movePosList.Add (Vector3.zero);
		}
		
		// v color
		float h = hsvColor.h;

		hsvColors = new List<HSVColor> ();
		for (int i = 0; i < 7; i++) {
			HSVColor c = new HSVColor (h, 0, 0);
			hsvColors.Add (c);
		}

		for( int i = 0; i < pointCount; i++){
			float s = Random.Range(0.5f, 1) * 0.5f;
			float b = Random.Range(0.3f, 1) * 0.5f;
			HSVColor c = new HSVColor (h, s, b);
			hsvColors.Add (c);
		}
		
		// mesh
		mesh = new Mesh ();
		mf.mesh = mesh;
	}

	// Use this for initialization
	void UpdateDelaunay () {

		delaunayTriangulation.Setup (rect);
		for (var i = 0; i < currentPosList.Count; i++) {
			delaunayTriangulation.Add (currentPosList[i]);
		}
	}

	List<Vector3> spList;

	[SerializeField]
	bool vertMode = false;

	List<Vector3> GetVertices()
	{
//		List<Vector3> t_vertices = delaunayTriangulation.GetVertices ();

		List<Vector3> t_vertices = new List<Vector3>();
		t_vertices.AddRange (delaunayTriangulation.GetSuperVertices ());
		t_vertices.AddRange (delaunayTriangulation.GetRectVertices ());
		t_vertices.AddRange (currentPosList);


		return t_vertices;
	}

	void DrawTriangles() {
		// rendering
		if (vertMode) {
			DrawVertMode ();
		}else{
			DrawPolyMode ();
		}
	}

	void DrawVertMode()
	{
		// unity
		// 頂点共通
		List<DelaunayTriangulation.Triangle> triangles = delaunayTriangulation.GetTriangles();
		List<Vector3> t_vertices = GetVertices ();

		List<int> tris = new List<int> ();

		for (var ti = 0; ti < triangles.Count; ti++) {
			DelaunayTriangulation.Triangle tri = triangles [ti];
			int[] triIndexs = tri.GetTriIndexs ();
			tris.AddRange (triIndexs);
		}

//		List<Color> color = new List<Color> ();
//		for (int i = 0; i < t_vertices.Count; i++) {
//			color.Add( colors[i] );
//		}
//		Debug.Log ("t_vertices: "+t_vertices.Count + " / "+color.Count + " / "+colors.Count + " / "+tris.Count);

		mesh.Clear ();

		mesh.vertices = t_vertices.ToArray ();
		mesh.triangles = tris.ToArray ();
		Color[] color = new Color[hsvColors.Count];
		for (int i = 0; i < hsvColors.Count; i++) {
			color [i] = hsvColors [i].GetColor ();
		}
		mesh.colors = color;

		mesh.RecalculateNormals ();

		mf.mesh = mesh;
	}

	void DrawPolyMode()
	{
		// unity
		// triごとに
		List<DelaunayTriangulation.Triangle> triangles = delaunayTriangulation.GetTriangles();
		List<Vector3> t_vertices = GetVertices ();

		int triCount = triangles.Count;
		
		Vector3[] vertices = new Vector3[triCount*3];
		int[] tris = new int[triCount*3];
		Color[] color = new Color[triCount*3];
		Vector3[] normals = new Vector3[triCount*3];

		int t_index = 0;
		for (int i = 0; i < triangles.Count; i++) {
			DelaunayTriangulation.Triangle tri = triangles[i];
			int[] triIndexs = tri.GetTriIndexs ();
			vertices[i*3+0] = (t_vertices[triIndexs[0]]);
			vertices[i*3+1] = (t_vertices[triIndexs[1]]);
			vertices[i*3+2] = (t_vertices[triIndexs[2]]);

			tris[i*3+0] = (t_index++);
			tris[i*3+1] = (t_index++);
			tris[i*3+2] = (t_index++);
			
			Color c = hsvColors [triIndexs [0]].GetColor() + hsvColors [triIndexs [1]].GetColor() + hsvColors [triIndexs [2]].GetColor();
			c = c / 3.0f;
			color[i*3+0] = (c);
			color[i*3+1] = (c);
			color[i*3+2] = (c);

			// normal
			Vector3 d1 = t_vertices[triIndexs[1]] - t_vertices[triIndexs[0]];
			Vector3 d2 = t_vertices[triIndexs[2]] - t_vertices[triIndexs[0]];
			Vector3 norm = Vector3.Cross (d1.normalized, d2.normalized);
			normals[i*3+0] = (norm);
			normals[i*3+1] = (norm);
			normals[i*3+2] = (norm);
		}
		//		Debug.Log ("t_vertices: "+vertices.Count + " / "+color.Count + " / "+tris.Count);

		mesh.Clear ();

		mesh.vertices = vertices;
		mesh.triangles = tris;
		mesh.colors = color;
		mesh.normals = normals;
	}

	// unity
	[SerializeField]
	MeshFilter mf;

	void Start () {
		Init ();
		UpdateDelaunay();
	}

	void Update()
	{
		// color
		float h = hsvColor.h;
		for(int i = 0; i < hsvColors.Count; i++){
			hsvColors [i].h = h;
		}

		// move
		for (int i = 0; i < orgPosList.Count; i++) {
			Vector3 orgPos = orgPosList[i];
			Vector3 move = movePosList [i];

			Vector3 sp = spList[i];
			
			move.x += sp.x * speed.x;
			move.y += sp.y * speed.y;
			move.z += sp.z * speed.z;
			Vector3 t_move = move;
			t_move.z *= ampMoveZ;

			Vector3 newPos = orgPos + t_move;
			newPos.x = Mathf.Clamp (newPos.x, rect.xMin, rect.xMax);
			newPos.y = Mathf.Clamp (newPos.y, rect.yMin, rect.yMax);

			if (newPos.x <= rect.xMin || newPos.x >= rect.xMax) {
				sp.x *= -1.0f;
				spList [i] = sp;

				if (newPos.x <= rect.xMin) {
					move.x = rect.xMin - orgPos.x;
				}
				if (newPos.x >= rect.xMax) {
					move.x = rect.xMax - orgPos.x;
				}
			}
			if(newPos.y <= rect.yMin || newPos.y >= rect.yMax){
				sp.y *= -1.0f;
				spList [i] = sp;

				if (newPos.y <= rect.yMin) {
					move.y = rect.yMin - orgPos.y;
				}
				if (newPos.y >= rect.yMax) {
					move.y = rect.yMax - orgPos.y;
				}
			}
			//
			float areaZ = 2.0f;
			if(move.z <= -areaZ || move.z >= areaZ){
				sp.z *= -1.0f;
				spList [i] = sp;

				if (newPos.z <= -areaZ) {
					move.z = -areaZ - orgPos.z;
				}
				if (newPos.z >= areaZ) {
					move.z = areaZ - orgPos.z;
				}

				move.z = Mathf.Clamp (move.z, -areaZ, areaZ);
			}

			movePosList[i] = move;
			currentPosList [i] = newPos;
		}

		if(Mathf.Abs(speed.x) > 0 || Mathf.Abs(speed.y) > 0){
			// 左右の移動がある時には、再分割を行う。
			UpdateDelaunay();
		}
		DrawTriangles();
	}
	
	// Update is called once per frame
	void OnDrawGizmos () {
		if (delaunayTriangulation == null) {
			return;
		}
		
//		List<Vector3> vs = delaunayTriangulation.GetVertices ();
//		for (int i = 0; i < vs.Count; i++) {
//			Gizmos.DrawSphere (this.transform.TransformPoint(vs [i]), 0.15f);
//		}
		for (int i = 0; i < currentPosList.Count; i++) {
			Gizmos.DrawSphere (this.transform.TransformPoint(currentPosList [i]), 0.15f);
		}

//		List<Vector3> svs = delaunayTriangulation.GetSuperVertices ();
//		Gizmos.color = Color.red;
//		for (int i = 0; i < svs.Count; i++) {
//			Gizmos.DrawSphere (svs [i], 0.5f);
//		}
		Gizmos.color = Color.green;
		Gizmos.DrawSphere (delaunayTriangulation.d_center, 0.15f);
	}
}
