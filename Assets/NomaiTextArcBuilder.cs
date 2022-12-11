using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public static class NomaiTextArcBuilder {
  public static SpiralMesh spiralMesh;
  public static GameObject spiralMeshHolder;
  public static int i = 0;

  public static int GetNumber()
  {
    if (!AssetDatabase.IsValidFolder("Assets/Spirals")) AssetDatabase.CreateFolder("Assets", "Spirals");
    var _exists = AssetDatabase.LoadAssetAtPath("Assets/Spirals/Spiral" + i + ".asset", typeof(Mesh)) as Mesh;
    if (_exists != null){
      i = i + 1;
      return GetNumber();
    }
    else{
      return i;
    }
  }

  public static void Place() {
    var gameObject = new GameObject("spiral holder");
    spiralMeshHolder = gameObject;

    // var rootArc = new SpiralTextArc();
    // rootArc.MakeChild();
    // spiralMesh = rootArc.m.children[0];
    // spiralMesh.updateChildren();

    var rootArc = new SpiralTextArc();
  }

  public static void RotatePlus()
  {
    if (spiralMesh == null) return;
    spiralMesh.a += 0.05f;
    spiralMesh.updateChildren();
  }

  public static void RotateMinus()
  {
    if (spiralMesh == null) return;
    spiralMesh.a -= 0.05f;
    spiralMesh.updateChildren();
  }

  public class SpiralTextArc {
    public GameObject g;
    public SpiralMesh m;

    public SpiralTextArc() {
      g = new GameObject();
      g.transform.parent = spiralMeshHolder.transform;
      g.transform.localPosition = Vector3.zero;
      g.transform.localEulerAngles = Vector3.zero;

      m = new SpiralMesh(adultSpiralProfile);
      m.Randomize();
      m.updateMesh();

      g.AddComponent<MeshFilter>().sharedMesh = m.mesh;
      g.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Sprites/Default"));
      g.GetComponent<MeshRenderer>().sharedMaterial.color = Color.magenta;
      g.AddComponent<NHNomaiTextLine>().SetModel(m);

      // (float arcLen, float offsetX, float offsetY, float offsetAngle, bool mirror, float scale, float a, float b, float startS)
      GameObject p = AddDebugShape.AddSphere(g, 0.05f, Color.green);
      Vector3 start = m.getDrawnSpiralPointAndNormal(m.endS);
      p.transform.localPosition = new Vector3(start.x, 0, start.y);
    }

    public SpiralTextArc MakeChild() {
      Debug.Log("MAKING CHILD");
      var s = new SpiralTextArc();
      s.m.startSOnParent = UnityEngine.Random.Range(50, 250);
      m.addChild(s.m);
      return s;
    }
  }

  public class NHNomaiTextLine : MonoBehaviour {
    List<Vector3> _points;
    List<float> normalAngles;

    public void SetModel(SpiralMesh model) {
      //
      // rotate to face up
      //
      
      var norm = model.skeleton[1] - model.skeleton[0];
      float r = Mathf.Atan2(-norm.y, norm.x) * Mathf.Rad2Deg;
      if (model.mirror) r += 180;
      transform.localEulerAngles = model.mirror
        ? new Vector3(0, 90-r, 0)
        : new Vector3(0, -90-r, 0);
      // var ang = model.mirror ? 90-r : -90-r;
      //model.ang = 180-r;
      //model.updateMesh();

      //
      // casche important stuff
      //

      this._points = model.skeleton.Select((compiled) => new Vector3(compiled.x, 0, compiled.y)).ToList();

      normalAngles = new List<float>();
      for (int i = 0; i<model.numSkeletonPoints; i++) {
        var normal = _points[Mathf.Min(i+1, model.numSkeletonPoints-1)] - _points[Mathf.Max(i-1, 0)];

        float rot = Mathf.Atan2(-normal.z, normal.x) * Mathf.Rad2Deg;
        if (model.mirror) rot += 180;

        normalAngles.Add(rot);
      }
      
      //
      // debug
      //

      for (int i = 0; i<model.numSkeletonPoints; i++) {
        GameObject j = AddDebugShape.AddSphere(this.gameObject, 0.05f, Color.green);
        j.transform.localPosition = _points[i];
      }
      
      for (int i = 0; i<model.numSkeletonPoints; i++) {
        GameObject s = AddDebugShape.AddStick(this.gameObject, Color.blue);
        s.transform.localPosition = _points[i];
        s.transform.localEulerAngles = new Vector3(0, normalAngles[i], 0);
        s.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
      }
    }
  }

  //
  //
  // Handle the connection between game objects and spiral meshes
  //
  //

  // TODO: spiral profiles, pass as a value to constructor, use value in Randomize()
  // use current defaults to make an AdultSpiralProfile, then make ChildSpiralProfile and StrangerSpiralProfile
  public struct SpiralProfile {
    // all of the Vector2 params here refer to a range of valid values
    public bool canMirror;
    public Vector2 a;
    public Vector2 b;
    public Vector2 endS;
    public Vector2 skeletonScale;
    public int numSkeletonPoints;
    public float uvScale;
    public float innerWidth;
    public float outerWidth;
    public Material material;
  }

  public static SpiralProfile adultSpiralProfile = new SpiralProfile() {
    canMirror = true,
      a = new Vector2(0.5f, 0.5f),
      b = new Vector2(0.3f, 0.6f),
      endS = new Vector2(0, 50f),
      skeletonScale = new Vector2(0.01f, 0.01f),
      numSkeletonPoints = 51,

      innerWidth = 0.001f, // width at the tip
      outerWidth = 0.05f, //0.107f; // width at the base
      uvScale = 4.9f, //2.9f;
  };

  //
  //
  // Construct spiral meshes from the mathematical spirals generated below
  //
  //

  public class SpiralMesh: Spiral {
    public new List<SpiralMesh> children = new List<SpiralMesh>();

    public List<Vector3> skeleton;
    public List<Vector2> skeletonOutsidePoints;

    public int numSkeletonPoints = 51; // seems to be Mobius' default

    public float innerWidth = 0.001f; // width at the tip
    public float outerWidth = 0.05f; //0.107f; // width at the base
    public float uvScale = 4.9f; //2.9f;
    private float baseUVScale = 1f / 300f;
    public float uvOffset = 0;

    public Mesh mesh;

    public SpiralMesh(SpiralProfile profile): base(profile) {
      this.numSkeletonPoints = profile.numSkeletonPoints;
      this.innerWidth = profile.innerWidth;
      this.outerWidth = profile.outerWidth;
      this.uvScale = profile.uvScale;

      this.uvOffset = UnityEngine.Random.value;
    }

    public override void Randomize() {
      base.Randomize();
      uvOffset = UnityEngine.Random.value;
      updateChildren();
    }

    internal void updateMesh() {
      skeleton = this.getSkeleton(numSkeletonPoints);
      skeletonOutsidePoints = this.getSkeletonOutsidePoints(numSkeletonPoints);
      
      List<Vector3> vertsSide1 = skeleton.Select((skeletonPoint, index) => {
        Vector3 normal = new Vector3(cos(skeletonPoint.z), 0, sin(skeletonPoint.z));
        float width = lerp(((float) index) / ((float) skeleton.Count()), outerWidth, innerWidth);

        return new Vector3(skeletonPoint.x, 0, skeletonPoint.y) + width * normal;
      }).ToList();

      List<Vector3> vertsSide2 = skeleton.Select((skeletonPoint, index) => {
        Vector3 normal = new Vector3(cos(skeletonPoint.z), 0, sin(skeletonPoint.z));
        float width = lerp(((float) index) / ((float) skeleton.Count()), outerWidth, innerWidth);

        return new Vector3(skeletonPoint.x, 0, skeletonPoint.y) - width * normal;
      }).ToList();

      Vector3[] newVerts = vertsSide1.Zip(vertsSide2, (f, s) => new [] {
       f,
        s
      }).SelectMany(f =>f).ToArray(); // interleave vertsSide1 and vertsSide2

      if (mesh != null && false) // TODO: remove the && false
      {
        mesh.vertices = newVerts;
        mesh.RecalculateBounds();
      } else {
        List<int> triangles = new List<int>();
        for (int i = 0; i<newVerts.Length - 2; i += 2) {
          /*  |  ⟍  |
              |    ⟍|
            2 *-----* 3                  
              |⟍    |                   
              |  ⟍  |        
              |    ⟍|                   
            0 *-----* 1       
              |⟍    | 
           */
          triangles.Add(i + 2);
          triangles.Add(i + 1);
          triangles.Add(i);

          triangles.Add(i + 2);
          triangles.Add(i + 3);
          triangles.Add(i + 1);
        }

        var startT = tFromArcLen(startS);
        var endT = tFromArcLen(endS);

        var rangeT = endT - startT;
        var rangeS = endS - startS;

        Vector2[] uvs = new Vector2[newVerts.Length];
        Vector2[] uv2s = new Vector2[newVerts.Length];
        for (int i = 0; i<skeleton.Count(); i++) {
          float fraction = 1 - ((float) i) / ((float) skeleton.Count()); // casting is so uuuuuuuugly

          // note: cutting the sprial into numPoints equal slices of arclen would
          // provide evenly spaced skeleton points
          // on the other hand, cutting the spiral into numPoints equal slices of t
          // will cluster points in areas of higher detail. this is the way Mobius does it, so it is the way we also will do it
          float inputT = startT + rangeT * fraction;
          float inputS = tToArcLen(inputT);
          float sFraction = (inputS - startS) / rangeS;
          float absoluteS = (inputS - startS);

          float u = absoluteS * uvScale * baseUVScale + uvOffset;
          uvs[i * 2] = new Vector2(u, 0);
          uvs[i * 2 + 1] = new Vector2(u, 1);

          uv2s[i * 2] = new Vector2(1 - sFraction, 0);
          uv2s[i * 2 + 1] = new Vector2(1 - sFraction, 1);
        }

        Vector3[] normals = new Vector3[newVerts.Length];
        for (int i = 0; i<newVerts.Length; i++) normals[i] = new Vector3(0, 1, 0);

        if (mesh == null){
          mesh = new Mesh(); // TODO: remove the if statement
          mesh.name = "Spiral" + GetNumber();
          AssetDatabase.CreateAsset(mesh, "Assets/Spirals/" + mesh.name + ".asset");
        }
        mesh.vertices = newVerts.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs;
        mesh.uv2 = uv2s;
        mesh.normals = normals;
        mesh.RecalculateBounds();
      }
    }

    internal void updateChild(SpiralMesh child, bool updateMesh = true) {
      Vector3 pointAndNormal = getDrawnSpiralPointAndNormal(child.startSOnParent);
      var cx = pointAndNormal.x;
      var cy = pointAndNormal.y;
      var cang = pointAndNormal.z + (this.mirror ? -1 : 1) * Mathf.PI / 2f; // if this spiral is mirrored, the child needs to be rotated by -90deg. if it's not, +90deg
      child.x = cx;
      child.y = cy;
      child.ang = cang + (child.mirror ? Mathf.PI : 0);

      if (updateMesh) child.updateMesh();
    }

    public void addChild(SpiralMesh child) {
      updateChild(child);
      this.children.Add(child);
    }

    public override void updateChildren() {
      this.updateMesh();
      this.children.ForEach(child => {
        updateChild(child, false);
        child.updateChildren();
      });
    }
  }

  //
  //
  // Construct the mathematical spirals that Nomai arcs are built from
  //
  //

  public class Spiral {
    public bool mirror;
    public float a;
    public float b; // 0.3-0.6
    public float startSOnParent;
    public float scale;
    public List<Spiral> children;

    public float x;
    public float y;
    public float ang;

    // public float startIndex = 2.5f;

    public float startS = 42.87957f; // go all the way down to 0, all the way up to 50
    public float endS = 342.8796f;

    SpiralProfile profile;

    public Spiral(SpiralProfile profile) {
      this.profile = profile;

      this.Randomize();
    }

    public Spiral(float startSOnParent = 0, bool mirror = false, float len = 300, float a = 0.5f, float b = 0.43f, float scale = 0.01f) {
      this.mirror = mirror;
      this.a = a;
      this.b = b;
      this.startSOnParent = startSOnParent;
      this.scale = scale;

      this.children = new List<Spiral>();

      this.x = 0;
      this.y = 0;
      this.ang = 0;
    }

    public virtual void Randomize() {
      this.a = UnityEngine.Random.Range(profile.a.x, profile.a.y); //0.5f;
      this.b = UnityEngine.Random.Range(profile.b.x, profile.b.y);
      this.startS = UnityEngine.Random.Range(profile.endS.x, profile.endS.y);
      this.scale = UnityEngine.Random.Range(profile.skeletonScale.x, profile.skeletonScale.y);
      if (profile.canMirror) this.mirror = UnityEngine.Random.value<0.5f;
    }

    internal virtual void updateChild(Spiral child) {
      Vector3 pointAndNormal = getDrawnSpiralPointAndNormal(child.startSOnParent);
      var cx = pointAndNormal.x;
      var cy = pointAndNormal.y;
      var cang = pointAndNormal.z;
      child.x = cx;
      child.y = cy;
      child.ang = cang + (child.mirror ? Mathf.PI : 0);
    }

    public virtual void addChild(Spiral child) {
      updateChild(child);
      this.children.Add(child);
    }

    public virtual void updateChildren() {
      this.children.ForEach(child => {
        updateChild(child);
        child.updateChildren();
      });
    }

    // note: each Vector3 in this list is of form <x, y, angle in radians of the normal at this point>
    public List<Vector3> getSkeleton(int numPoints) {
      var endT = tFromArcLen(endS);
      var startT = tFromArcLen(startS);
      var rangeT = endT - startT;

      List<Vector3> skeleton = new List<Vector3>();
      for (int i = 0; i<numPoints; i++) {
        float fraction = ((float) i) / ((float) numPoints - 1f); // casting is so uuuuuuuugly

        // note: cutting the sprial into numPoints equal slices of arclen would
        // provide evenly spaced skeleton points
        // on the other hand, cutting the spiral into numPoints equal slices of t
        // will cluster points in areas of higher detail. this is the way Mobius does it, so it is the way we also will do it
        float inputT = startT + rangeT * fraction;
        float inputS = tToArcLen(inputT);

        skeleton.Add(getDrawnSpiralPointAndNormal(inputS));
      }

      skeleton.Reverse();
      return skeleton;
    }

    public List<Vector2> getSkeletonOutsidePoints(int numPoints) {
      var endT = tFromArcLen(endS);
      var startT = tFromArcLen(startS);
      var rangeT = endT - startT;

      List<Vector2> outsidePoints = new List<Vector2>();
      for (int i = 0; i<numPoints; i++) {
        float fraction = ((float) i) / ((float) numPoints - 1f); // casting is so uuuuuuuugly

        // note: cutting the sprial into numPoints equal slices of arclen would
        // provide evenly spaced skeleton points
        // on the other hand, cutting the spiral into numPoints equal slices of t
        // will cluster points in areas of higher detail. this is the way Mobius does it, so it is the way we also will do it
        float inputT = startT + rangeT * fraction;
        float inputS = tToArcLen(inputT);

        var point = (getDrawnSpiralPointAndNormal(inputS));

        
        var deriv = spiralDerivative(inputT);
        var outsidePoint = new Vector2(point.x, point.y) - (new Vector2(-deriv.y, deriv.x)).normalized * 0.1f;
        outsidePoints.Add(outsidePoint);
      }

      outsidePoints.Reverse();
      return outsidePoints;
    }

    // all of this math is based off of this:
    // https://www.desmos.com/calculator/9gdfgyuzf6
    //
    // note: t refers to theta, and s refers to arc length
    //

    // get the (x, y) coordinates and the normal angle at the given location (measured in arcLen) of a spiral with the given parameters 
    // note: arcLen is inverted so that 0 refers to what we consider the start of the Nomai spiral
    public Vector3 getDrawnSpiralPointAndNormal(float arcLen) {
      float offsetX = this.x;
      float offsetY = this.y;
      float offsetAngle = this.ang;
      var startS = this.endS; // I know this is funky, but just go with it for now. 

      var startT = tFromArcLen(startS); // this is the `t` value for the root of the spiral (the end of the non-curled side)

      var startPoint = spiralPoint(startT); // and this is the (x,y) location of the non-curled side, relative to the rest of the spiral. we'll offset everything so this is at (0,0) later
      var startX = startPoint.x;
      var startY = startPoint.y;

      var t = tFromArcLen(arcLen);
      var point = spiralPoint(t); // the absolute (x,y) location that corresponds to `arcLen`, before accounting for things like putting the start point at (0,0), or dealing with offsetX/offsetY/offsetAngle
      var x = point.x;
      var y = point.y;
      var ang = normalAngle(t);

      if (mirror) {
        x = x + 2 * (startX - x);
        ang = -ang + Mathf.PI;
      }

      // translate so that startPoint is at (0,0)
      // (also scale the spiral)
      var retX = scale * (x - startX);
      var retY = scale * (y - startY);

      // rotate offsetAngle rads 
      var retX2 = retX * cos(offsetAngle) -
        retY * sin(offsetAngle);
      var retY2 = retX * sin(offsetAngle) +
        retY * cos(offsetAngle);

      retX = retX2;
      retY = retY2;

      // translate for offsetX, offsetY
      retX += offsetX;
      retY += offsetY;

      return new Vector3(retX, retY, ang + offsetAngle + Mathf.PI / 2f);
    }

    // the base formula for the spiral
    public Vector2 spiralPoint(float t) {
      var r = a * exp(b * t);
      var retval = new Vector2(r * cos(t), r * sin(t));
      return retval;
    }

    // the spiral's got two functions: x(t) and y(t)
    // so it's got two derrivatives (with respect to t) x'(t) and y'(t)
    public Vector2 spiralDerivative(float t) { // derrivative with respect to t
      var r = a * exp(b * t);
      return new Vector2(
        -r * (sin(t) - b * cos(t)),
        r * (b * sin(t) + cos(t))
      );
    }

    // returns the length of the spiral between t0 and t1
    public float spiralArcLength(float t0, float t1) {
      return (a / b) * sqrt(b * b + 1) * (exp(b * t1) - exp(b * t0));
    }

    // converts from a value of t to the equivalent value of s (the value of s that corresponds to the same point on the spiral as t)
    public float tToArcLen(float t) {
      return spiralArcLength(0, t);
    }

    // reverse of above
    public float tFromArcLen(float s) {
      return ln(
        1 + s / (
          (a / b) *
          sqrt(b * b + 1)
        )
      ) / b;
    }

    // returns the angle of the spiral's normal at a given point
    public float normalAngle(float t) {
      var d = spiralDerivative(t);
      var n = new Vector2(d.y, -d.x);
      var angle = Mathf.Atan2(n.y, n.x);

      return angle - Mathf.PI / 2;
    }
  }

  // convenience, so the math above is more readable
  private static float lerp(float a, float b, float t) {
    return a * t + b * (1 - t);
  }

  private static float cos(float t) {
    return Mathf.Cos(t);
  }
  private static float sin(float t) {
    return Mathf.Sin(t);
  }
  private static float exp(float t) {
    return Mathf.Exp(t);
  }
  private static float sqrt(float t) {
    return Mathf.Sqrt(t);
  }
  private static float ln(float t) {
    return Mathf.Log(t);
  }

  public static class AddDebugShape
  {
    public static GameObject AddSphere(GameObject obj, float radius, Color color)
    {
      var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      sphere.transform.name = "DebugSphere";

      try
      {
        sphere.GetComponent<SphereCollider>().enabled = false;
        sphere.transform.parent = obj.transform;
        sphere.transform.localScale = new Vector3(radius, radius, radius);

        sphere.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Sprites/Default"));
        sphere.GetComponent<MeshRenderer>().sharedMaterial.color = color;
      }
      catch
      {
        // Something went wrong so make sure the sphere is deleted
        GameObject.Destroy(sphere);
      }

      return sphere.gameObject;
    }
  
    public static GameObject AddStick(GameObject obj,  Color color)
    {
      var width = 0.1f;
      var length = 0.5f;

      var newVerts = new Vector3[] {
        new Vector3(-width/2f, 0, 0),
        new Vector3(width/2f, 0, 0),
        new Vector3(-width/2f, 0, length),
        new Vector3(width/2f, 0, length),
      };

      /*
        2 *-----* 3                  
          |⟍    |                   
          |  ⟍  |        
          |    ⟍|                   
        0 *-----* 1       
        */
      var triangles = new List<int>();
      triangles.Add(0 + 2);
      triangles.Add(0 + 1);
      triangles.Add(0);

      triangles.Add(0 + 2);
      triangles.Add(0 + 3);
      triangles.Add(0 + 1);

      Vector3[] normals = new Vector3[newVerts.Length];
      for (int i = 0; i<newVerts.Length; i++) normals[i] = new Vector3(0, 1, 0);

      var mesh = new Mesh();
      mesh.vertices = newVerts;
      mesh.triangles = triangles.ToArray();
      mesh.normals = normals;
      mesh.RecalculateBounds();

      var g = new GameObject();
      g.AddComponent<MeshFilter>().sharedMesh = mesh;
      g.AddComponent<MeshRenderer>();
      g.transform.parent = obj.transform;
      g.GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Sprites/Default"));
      g.GetComponent<MeshRenderer>().sharedMaterial.color = color;

      return g;
    }
  }
}