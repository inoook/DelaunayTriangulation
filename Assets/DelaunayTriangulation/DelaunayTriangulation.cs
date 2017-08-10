using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://www.openprocessing.org/sketch/404200
// http://30min-processing.hatenablog.com/entry/2017/02/08/000000

public class DelaunayTriangulation {

	#region static utils
	static float sq(float v)
	{
		return v * v;
	}
	static Vector3 createVector(float x, float y)
	{
		return new Vector3 (x, y, 0);
	}
	static float sqrt(float v)
	{
		return Mathf.Sqrt (v);
	}
	#endregion

	#region tri
	public class Triangle
	{
		DelaunayTriangulation dt;

		int[] triIndexs;
		
		public Triangle(DelaunayTriangulation dt_, int id1, int id2, int id3)
		{
			dt = dt_;
			triIndexs = new int[3]{id1, id2, id3};
			List<Vector3> vertices = dt.vertices;
			Vector3 v1 = vertices[id1];
			Vector3 v2 = vertices[id2];
			Vector3 v3 = vertices[id3];

			float c = 2.0f * ((v2.x - v1.x) * (v3.y - v1.y) - (v2.y - v1.y) * (v3.x - v1.x));
			float x = ((v3.y - v1.y) * (sq(v2.x) - sq(v1.x) + sq(v2.y) - sq(v1.y)) + (v1.y - v2.y) * (sq(v3.x) - sq(v1.x) + sq(v3.y) - sq(v1.y))) / c;
			float y = ((v1.x - v3.x) * (sq(v2.x) - sq(v1.x) + sq(v2.y) - sq(v1.y)) + (v2.x - v1.x) * (sq(v3.x) - sq(v1.x) + sq(v3.y) - sq(v1.y))) / c;
			Vector3 center = createVector(x, y);
			v1.z = 0;
			center.z = 0;
			float radiuqSqr = Vector3.SqrMagnitude(v1 - center);

			this.center = center;
			this.sqrRadius = radiuqSqr;
		}

		
		public List<Triangle> Divide(int newIndex) {
			List<Triangle> tris = new List<Triangle> ();
			for (int i = 0; i < 3; i++) {
				int j = (i == 2) ? 0: i + 1;
				Triangle tri = new Triangle (dt, triIndexs[i], triIndexs[j], newIndex);
				tris.Add(tri);
			}
			return tris;
		}

		public bool IsContain(int index) {
			for (int i = 0; i < 3; i++) {
				if(triIndexs[i] == index){
					return true;
				}
			}
			return false;
		}
		
		public int[] GetTriIndexs()
		{
			return triIndexs;
		}

		// circle
		Vector3 center;
		float sqrRadius;
		
		public bool IsInCircle(Vector3 v) {
			v.z = 0;
			center.z = 0;
			return Vector3.SqrMagnitude(v - center) < sqrRadius;
		}
	}
	#endregion

	#region DelaunayTriangulation

	List<Triangle> triangles;
	public List<Vector3> vertices;

	List<Vector3> superVertices;
	List<Vector3> rectVertices;

	public Vector3 d_center; // debug

	public DelaunayTriangulation()
	{
		superVertices = new List<Vector3> ();
		vertices = new List<Vector3> ();
		triangles = new List<Triangle> ();

		rectVertices = new List<Vector3>();
	}

	public void Setup(Rect rect)
	{
		Clear ();

		Vector3 center = rect.center;
		float width = rect.width;
		float height = rect.height;

		d_center = center;
		float radius = sqrt(sq(width) + sq(height))/2.0f * 1.25f;

		Vector3 v1 = createVector(center.x - sqrt(3) * radius, center.y - radius);
		Vector3 v2 = createVector(center.x + sqrt(3) * radius, center.y - radius);
		Vector3 v3 = createVector(center.x, center.y +  2.0f * radius);
		
//		superVertices = new List<Vector3> ();
		superVertices.Add(v1);
		superVertices.Add(v2);
		superVertices.Add(v3);

//		vertices = new List<Vector3> ();
		vertices.Add(v1);
		vertices.Add(v2);
		vertices.Add(v3);

		Triangle t = new Triangle(this, 0, 1, 2);
		triangles.Add(t);
		
		// Area rect
		rectVertices.Add((createVector(rect.xMin, rect.yMin)));
		rectVertices.Add((createVector(rect.xMax, rect.yMin)));
		rectVertices.Add((createVector(rect.xMax, rect.yMax)));
		rectVertices.Add((createVector(rect.xMin, rect.yMax)));

		Add(rectVertices[0]);
		Add(rectVertices[1]);
		Add(rectVertices[2]);
		Add(rectVertices[3]);
	}

	void Clear()
	{
		superVertices.Clear ();
		vertices.Clear ();
		triangles.Clear ();
		rectVertices.Clear ();
	}
	
	public void Add(Vector3 v)
	{
		int vIndex = this.vertices.Count;

		// addvertex
		this.vertices.Add(v);

		List<Triangle> nextTriangles = new List<Triangle> ();
		List<Triangle> newTriangles = new List<Triangle> ();
		for (int ti = 0; ti < this.triangles.Count; ti++) {
			Triangle tri = this.triangles[ti];
			if(tri.IsInCircle(v)) {
				newTriangles.AddRange(tri.Divide(vIndex));
			} else {
				nextTriangles.Add(tri);
			}
		}

		for (int ti = 0; ti < newTriangles.Count; ti++) {
			Triangle tri = newTriangles[ti];
			bool isIllegal = false;
			for (int vi = 0; vi < this.vertices.Count; vi++) {
				if (this.IsIllegalTriangle(tri, vi)) {
					isIllegal = true;
					break;
				}
			}
			if (!isIllegal) {
				nextTriangles.Add(tri);
			}
		}
		this.triangles = nextTriangles; 
	}

	public List<Triangle> GetTriangles(){
//		return triangles;
		
		List<Triangle> ts = new List<Triangle> ();
		for (int ti = 0; ti < this.triangles.Count; ti++) {
			Triangle t = triangles[ti];
			bool hasSuperVertex = false;
			for (int vi = 0; vi < 3; vi++) {
				if (t.IsContain(vi)) {
					hasSuperVertex = true;
				}
			}
			if (!hasSuperVertex) {
				ts.Add(t);
			}
		}
		return ts;
	}

	bool IsIllegalTriangle(Triangle t, int index) {
		if(t.IsContain(index)) {
			return false;
		}
		Vector3 v = vertices[index];
		return t.IsInCircle(v);
	}

	List<Triangle> GetTrianglesWithSuperTriangle() {
		return this.triangles;
	}

	public List<Vector3> GetVertices()
	{
		return this.vertices;
	}

	public List<Vector3> GetSuperVertices()
	{
		return this.superVertices;
	}
	public List<Vector3> GetRectVertices()
	{
		return this.rectVertices;
	}
	#endregion
}
