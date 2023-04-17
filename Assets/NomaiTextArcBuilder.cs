using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Reflection;

public static class NomaiTextArcBuilder {
  public static int i = 0;

  public static string arcType;
  public static SpiralProfile spiralProfile;
  public static bool removeBakedInRotationAndPosition = true;

  public static int MIN_PARENT_POINT = 3;
  public static int MAX_PARENT_POINT = 26;
  
  public static GameObject Place(GameObject spiralMeshHolder = null) {
    if (spiralMeshHolder == null) 
    {
      spiralMeshHolder = new GameObject("spiral holder");
      spiralMeshHolder.AddComponent<SpiralArranger>();
    }
    
    arcType = "Adult";
    spiralProfile = adultSpiralProfile;

    var rootArc = new SpiralTextArc();
    rootArc.g.transform.parent = spiralMeshHolder.transform;
    rootArc.g.transform.localEulerAngles = new Vector3(0, 0, Random.Range(-60, 60));

    var manip = rootArc.g.AddComponent<SpiralManipulator>();
    if (Random.value < 0.5) manip.transform.localScale = new Vector3(-1, 1, 1); // randomly mirror
    spiralMeshHolder.GetComponent<SpiralArranger>().spirals.Add(manip);

    return rootArc.g;
  }
    

  public static void PlaceAdult() 
  { 
    arcType = "Adult";
    spiralProfile = adultSpiralProfile;

    var rootArc = new SpiralTextArc();
    rootArc.g.name = "Text Arc Prefab " + (i++);
  }
  public static void PlaceChild() {
    arcType = "Child";
    spiralProfile = childSpiralProfile;

    var rootArc = new SpiralTextArc();
    rootArc.g.name = "Text Arc Prefab " + (i++);
  }

  [ExecuteInEditMode]
  public class SpiralArranger : MonoBehaviour {
    public List<SpiralManipulator> spirals = new List<SpiralManipulator>();
    private HashSet<int> spiralsThatHaveBeenMirrored = new HashSet<int>();
    private Dictionary<int, int> sprialOverlapResolutionPriority = new Dictionary<int, int>();

    public float maxX = 4;
    public float minX = -4;
    public float maxY = 5f;
    public float minY = -1f;

    public int SPIRAL_OF_INTEREST = 6;

    private void OnDrawGizmosSelected() 
    {
      var topLeft     = new Vector3(minX, maxY) + transform.position;
      var topRight    = new Vector3(maxX, maxY) + transform.position;
      var bottomRight = new Vector3(maxX, minY) + transform.position;
      var bottomLeft  = new Vector3(minX, minY) + transform.position;
      Debug.DrawLine(topLeft, topRight, Color.red);
      Debug.DrawLine(topRight, bottomRight, Color.red);
      Debug.DrawLine(bottomRight, bottomLeft, Color.red);
      Debug.DrawLine(bottomLeft, topLeft, Color.red);
    }

    public int AttemptOverlapResolution(Vector2Int overlappingSpirals) 
    {
        if (!sprialOverlapResolutionPriority.ContainsKey(overlappingSpirals.x)) sprialOverlapResolutionPriority[overlappingSpirals.x] = 0;
        if (!sprialOverlapResolutionPriority.ContainsKey(overlappingSpirals.y)) sprialOverlapResolutionPriority[overlappingSpirals.y] = 0;

        int mirrorIndex = overlappingSpirals.x;
        if (sprialOverlapResolutionPriority[overlappingSpirals.y] > sprialOverlapResolutionPriority[overlappingSpirals.x]) mirrorIndex = overlappingSpirals.y;

        this.spirals[mirrorIndex].Mirror();
        sprialOverlapResolutionPriority[mirrorIndex]--;

        return mirrorIndex;
    }

    public Vector2Int Overlap() 
    {
      var index = -1;
      foreach (var s1 in spirals) 
      {
        index++;
        if (s1.parent == null) continue;

        var jndex = -1;
        foreach (var s2 in spirals) 
        {
          jndex++;
          if (s1 == s2) continue;
          //if (s1.parent == s2) continue;
          //if (s1 == s2.parent) continue;

          if (Vector3.Distance(s1.center, s2.center) > Mathf.Max(s1.NomaiTextLine.GetWorldRadius(), s2.NomaiTextLine.GetWorldRadius())) continue; // no overlap possible - too far away

          var s1Points = s1.NomaiTextLine.GetPoints().Select(p => s1.transform.TransformPoint(p)).ToList();
          var s2Points = s2.NomaiTextLine.GetPoints().Select(p => s2.transform.TransformPoint(p)).ToList();
          var s1ThresholdForOverlap = Vector3.Distance(s1Points[0], s1Points[1]);
          var s2ThresholdForOverlap = Vector3.Distance(s2Points[0], s2Points[1]);
          var thresholdForOverlap = Mathf.Pow(Mathf.Max(s1ThresholdForOverlap, s2ThresholdForOverlap), 2); // square to save on computation (we'll work in distance squared from here on)

          if (s1.parent == s2) s1Points.RemoveAt(0); // don't consider the base points so that we can check if children overlap their parents 
          if (s2.parent == s1) s2Points.RemoveAt(0); // (note: the base point of a child is always exactly overlapping with one of the parent's points)

          foreach(var p1 in s1Points)
          {
            foreach(var p2 in s2Points)
            {
                if (Vector3.SqrMagnitude(p1-p2) <= thresholdForOverlap) return new Vector2Int(index, jndex); // s1 and s2 overlap
            }
          }
        }
      }

      return new Vector2Int(-1, -1);
    }

    public void Step() {
      // TODO: after setting child position on parent in Step(), check to see if this spiral exits the bounds - if so, move it away until it no longer does
      // this ensures that a spiral can never be outside the bounds, it makes them rigid

      // TODO: for integration with NH - before generating spirals, seed the RNG with the hash of the XML filename for this convo
      // and add an option to specify the seed

      var index = -1;
      foreach (var s1 in spirals) 
      {
        index++;
        if (s1.parent == null) continue;

        Vector2 force = Vector2.zero;
        foreach (var s2 in spirals) 
        {
          if (s1 == s2) continue;
          if (s1.parent == s2) continue;
          if (s1 == s2.parent) continue;
          
          // push away from other spirals
          var f = (s2.center - s1.center);
          force -= f / Mathf.Pow(f.magnitude, 6);

          var f2 = (s2.localPosition - s1.localPosition);
          force -= f2 / Mathf.Pow(f2.magnitude, 6);

          //// account for spirals that get locked together (this happens when a mirrored spiral and non-mirrored spiral are close enough together that one spiral has its base to the left of the other's base and its center to the right of the other's center)
          //if (Vector2.Angle(f, f2) > 90) 
          //{
          //  s1.transform.localScale = new Vector3(-s1.transform.localScale.x, 1, 1);
          //  force = Vector2.zero;
          //  break;
          //}
          
          if (index == SPIRAL_OF_INTEREST) Debug.DrawLine(s2.center, s1.center);
        }
        
        
        // push away from the edges
        if (s1.center.y < minY+s1.transform.parent.position.y) force += new Vector2(0, Mathf.Pow(10f*minY - 10f*s1.center.y, 6));
        if (s1.center.x < minX+s1.transform.parent.position.x) force += new Vector2(Mathf.Pow(10f*minX - 10f*s1.center.x, 6), 0);
        if (s1.center.y > maxY+s1.transform.parent.position.y) force -= new Vector2(0, Mathf.Pow(10f*maxY - 10f*s1.center.y, 6));
        if (s1.center.x > maxX+s1.transform.parent.position.x) force -= new Vector2(Mathf.Pow(10f*maxX - 10f*s1.center.x, 6), 0);

        //
        // renormalize the force magnitude (keeps force sizes reasonable, and improves stability in the case of small forces)
        //

        var avg = 1; // the size of vector required to get a medium push
        var scale = 0.75f;
        force = force.normalized * scale * (1 / (1 + Mathf.Exp(avg-force.magnitude)) - 1 / (1 + Mathf.Exp(avg))); // apply a sigmoid-ish smoothing operation, so only giant forces actually move the spirals

        //
        // apply the forces as we go to increase stability?
        //

        var spiral = s1;
        var parentPoints = spiral.parent.GetComponent<NomaiTextLine>().GetPoints();
        
        // pick the parent point that's closest to center+force, and move to there
        var idealPoint = spiral.position + force;
        var bestPointIndex = 0;
        var bestPointDistance = 99999999f;
        for (var j = MIN_PARENT_POINT; j < MAX_PARENT_POINT; j++) 
        {
          // skip this point if it's already occupied by ANOTHER spiral (if it's occupied by this spiral, DO count it)
          if (j != spiral._parentPointIndex && spiral.parent.occupiedParentPoints.Contains(j)) continue;

          var point = parentPoints[j];
          point = spiral.parent.transform.TransformPoint(point);

          var dist = Vector2.Distance(point, idealPoint);
          if (dist < bestPointDistance) {
            bestPointDistance = dist;
            bestPointIndex = j;
          }
        }
        
        //
        // limit the distance a spiral can move in a single step
        //

        var MAX_MOVE_DISTANCE = 2;
        bestPointIndex = spiral._parentPointIndex + Mathf.Min(MAX_MOVE_DISTANCE, Mathf.Max(-MAX_MOVE_DISTANCE, bestPointIndex - spiral._parentPointIndex)); // minimize step size to help stability
        
        //
        // actually move the spiral
        //

        SpiralManipulator.PlaceChildOnParentPoint(spiral.gameObject, spiral.parent.gameObject, bestPointIndex);
      }
    }
  }

  [ExecuteInEditMode]
  public class SpiralManipulator : MonoBehaviour {
    public SpiralManipulator parent;
    public List<SpiralManipulator> children = new List<SpiralManipulator>();

    public HashSet<int> occupiedParentPoints = new HashSet<int>();
    public int _parentPointIndex = -1;

    public Vector2 localPosition {
        get { return new Vector2(this.transform.localPosition.x, this.transform.localPosition.y); }
    }
    public Vector2 position {
        get { return new Vector2(this.transform.position.x, this.transform.position.y); }
    }

    
    private NomaiTextLine _NomaiTextLine;
    public NomaiTextLine NomaiTextLine 
    {
        get 
        {
            if (_NomaiTextLine == null) _NomaiTextLine = GetComponent<NomaiTextLine>();
            return _NomaiTextLine;
        }
    }

    public Vector2 center { get { return NomaiTextLine.GetWorldCenter(); } }

    public SpiralManipulator AddChild() {
      var index = Random.Range(MIN_PARENT_POINT, MAX_PARENT_POINT);
      GameObject child = Place(this.transform.parent.gameObject);
      PlaceChildOnParentPoint(child, this.gameObject, index);

      child.GetComponent<SpiralManipulator>().parent = this;
      this.children.Add(child.GetComponent<SpiralManipulator>());
      return child.GetComponent<SpiralManipulator>();
    }

    public void Mirror() 
    {       
        this.transform.localScale = new Vector3(-this.transform.localScale.x, 1, 1);
        if (this.parent != null) SpiralManipulator.PlaceChildOnParentPoint(this.gameObject, this.parent.gameObject, this._parentPointIndex);
    }

    public static int PlaceChildOnParentPoint(GameObject child, GameObject parent, int parentPointIndex, bool updateChildren=true) 
    {
      var childManipulator = child.GetComponent<SpiralManipulator>();
      var parentManipulator = parent.GetComponent<SpiralManipulator>();

      // track which points on the parent are being occupied
      if (childManipulator._parentPointIndex != -1) parentManipulator.occupiedParentPoints.Remove(childManipulator._parentPointIndex);
      childManipulator._parentPointIndex = parentPointIndex; // just in case this function was called without setting this value
      parentManipulator.occupiedParentPoints.Add(parentPointIndex);

      // get the parent's points and make parentPointIndex valid
      var _points = parent.GetComponent<NomaiTextLine>().GetPoints();
      parentPointIndex = Mathf.Max(0, Mathf.Min(parentPointIndex, _points.Length-1));

      // calculate the normal at point by using the neighboring points to approximate the tangent (and account for mirroring, which means all points are actually at (-point.x, point.y) )
      var normal = _points[Mathf.Min(parentPointIndex+1, _points.Length-1)] - _points[Mathf.Max(parentPointIndex-1, 0)];
      if (parent.transform.localScale.x < 0) normal = new Vector3(-normal.x, normal.y, normal.z);
      float rot = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg;
      if (parent.transform.localScale.x < 0) rot += 180; // account for mirroring again (without doing this, the normal points inward on mirrored spirals, instead of outward)

      // get the location the child spiral should be at (and yet again account for mirroring)
      var point = _points[parentPointIndex];
      if (parent.transform.localScale.x < 0) point = new Vector3(-point.x, point.y, point.z);

      // set the child's position and rotation according to calculations
      child.transform.localPosition = Quaternion.Euler(0, 0, parent.transform.localEulerAngles.z) * point + parent.transform.localPosition;
      child.transform.localEulerAngles = new Vector3(0, 0, rot + parent.transform.localEulerAngles.z);

      // recursive update on all children so they move along with the parent
      if (updateChildren) 
      { 
        foreach(var grandchild in childManipulator.children) 
        {
            PlaceChildOnParentPoint(grandchild.gameObject, child, grandchild._parentPointIndex);
        }
      }

      return parentPointIndex;
    }
  }

  public class SpiralTextArc {
    public GameObject g;
    public SpiralMesh m;

    public SpiralTextArc() {
      g = new GameObject("New Nomai Spiral");
      g.transform.localPosition = Vector3.zero;
      g.transform.localEulerAngles = Vector3.zero;

      m = new SpiralMesh(NomaiTextArcBuilder.spiralProfile);
      m.Randomize();
      m.updateMesh();

      g.AddComponent<MeshFilter>().sharedMesh = m.mesh;
      g.AddComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Sprites/Default"));
      g.GetComponent<MeshRenderer>().sharedMaterial.color = Color.magenta;

      var owNomaiTextLine = g.AddComponent<NomaiTextLine>();

      //
      // rotate mesh to face up
      //
      
      var norm = m.skeleton[1] - m.skeleton[0];
      float r = Mathf.Atan2(-norm.y, norm.x) * Mathf.Rad2Deg;
      if (m.mirror) r += 180;
      var ang = m.mirror ? 90-r : -90-r;

      // using m.sharedMesh causes old meshes to disappear for some reason, idk why
      var mesh = g.GetComponent<MeshFilter>().mesh;
      if (removeBakedInRotationAndPosition)
      {
          var meshCopy = mesh;
          var newVerts = meshCopy.vertices.Select(v => Quaternion.Euler(-90, 0, 0) * Quaternion.Euler(0, ang, 0) * v).ToArray();
          meshCopy.vertices = newVerts;
          meshCopy.RecalculateBounds();
      }

      AssetDatabase.CreateAsset(mesh, "Assets/Spirals/"+(NomaiTextArcBuilder.arcType)+"spiral" + (NomaiTextArcBuilder.i) + ".asset");
      g.GetComponent<MeshFilter>().sharedMesh = AssetDatabase.LoadAssetAtPath("Assets/Spirals/"+(NomaiTextArcBuilder.arcType)+"spiral" + (NomaiTextArcBuilder.i) + ".asset", typeof(Mesh)) as Mesh;
      NomaiTextArcBuilder.i++;

      //
      // cache important stuff
      //

      var _points = m.skeleton
        .Select((compiled) => 
          Quaternion.Euler(-90, 0, 0) * Quaternion.Euler(0, ang, 0) * (new Vector3(compiled.x, 0, compiled.y)) // decompile them, rotate them by ang, and then rotate them to be vertical, like the base game spirals are
        )
        .ToList();

      var normalAngles = new List<float>();
      for (int i = 0; i<m.numSkeletonPoints; i++) {
        var normal = _points[Mathf.Min(i+1, m.numSkeletonPoints-1)] - _points[Mathf.Max(i-1, 0)];

        float rot = Mathf.Atan2(normal.y, normal.x) * Mathf.Rad2Deg - 90;
        if (m.mirror) rot += 180;

        normalAngles.Add(rot);
      }
      
      //
      // set up NomaiTextArc stuff
      //

      var _lengths = _points.Take(_points.Count()-1).Select((point, i) => Vector3.Distance(point, _points[i+1])).ToArray();
      var _totalLength = _lengths.Aggregate(0f, (acc, length) => acc + length);
      var _state = NomaiTextLine.VisualState.UNREAD;
      var _textLineLocation = NomaiText.Location.UNSPECIFIED;
      var _center = _points.Aggregate(Vector3.zero, (acc, point) => acc + point) / (float)_points.Count();
      var _radius = _points.Aggregate(0f,           (acc, point) => Mathf.Max(Vector3.Distance(_center, point), acc));
      var _active = true;

      (typeof (NomaiTextLine)).InvokeMember("_points", BindingFlags.SetField | BindingFlags.Instance | BindingFlags.NonPublic, null, owNomaiTextLine, new object[] { _points.ToArray() });
      (typeof (NomaiTextLine)).InvokeMember("_lengths", BindingFlags.SetField | BindingFlags.Instance | BindingFlags.NonPublic, null, owNomaiTextLine, new object[] { _lengths });
      (typeof (NomaiTextLine)).InvokeMember("_totalLength", BindingFlags.SetField | BindingFlags.Instance | BindingFlags.NonPublic, null, owNomaiTextLine, new object[] { _totalLength });
      (typeof (NomaiTextLine)).InvokeMember("_state", BindingFlags.SetField | BindingFlags.Instance | BindingFlags.NonPublic, null, owNomaiTextLine, new object[] { _state });
      (typeof (NomaiTextLine)).InvokeMember("_textLineLocation", BindingFlags.SetField | BindingFlags.Instance | BindingFlags.NonPublic, null, owNomaiTextLine, new object[] { _textLineLocation });
      (typeof (NomaiTextLine)).InvokeMember("_center", BindingFlags.SetField | BindingFlags.Instance | BindingFlags.NonPublic, null, owNomaiTextLine, new object[] { _center });
      (typeof (NomaiTextLine)).InvokeMember("_radius", BindingFlags.SetField | BindingFlags.Instance | BindingFlags.NonPublic, null, owNomaiTextLine, new object[] { _radius });
      (typeof (NomaiTextLine)).InvokeMember("_active", BindingFlags.SetField | BindingFlags.Instance | BindingFlags.NonPublic, null, owNomaiTextLine, new object[] { _active });
    }
  }

  //
  //
  // Handle the connection between game objects and spiral meshes
  //
  //

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
    canMirror = false, // we don't want to mirror the actual mesh itself anymore, we'll just mirror the game object using localScale.x
    a = new Vector2(0.5f, 0.5f),
    b = new Vector2(0.3f, 0.6f),
    endS = new Vector2(0, 50f),
    skeletonScale = new Vector2(0.01f, 0.01f),
    numSkeletonPoints = 51,

    innerWidth = 0.001f, // width at the tip
    outerWidth = 0.05f, // width at the base
    uvScale = 4.9f,
  };

  public static SpiralProfile childSpiralProfile = new SpiralProfile() {
    canMirror = false, // we don't want to mirror the actual mesh itself anymore, we'll just mirror the game object using localScale.x
    a = new Vector2(0.9f, 0.9f),
    b = new Vector2(0.305f, 0.4f),
    endS = new Vector2(16f, 60f), 
    skeletonScale = new Vector2(0.002f, 0.005f),
    numSkeletonPoints = 51,

    innerWidth = 0.001f/10f, // width at the tip
    outerWidth = 2f*0.05f,  // width at the base
    uvScale = 4.9f/3.5f, 
  };

  //
  //
  // Construct spiral meshes from the mathematical spirals generated below
  //
  //

  public class SpiralMesh: Spiral {
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
      uvOffset = UnityEngine.Random.value; // this way even two spirals that are exactly the same shape will look different (this changes the starting point of the handwriting texture)
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
      
      List<int> triangles = new List<int>();
      for (int i = 0; i<newVerts.Length - 2; i += 2) {
          /*
            |  ⟍  |
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
      for (int i = 0; i<newVerts.Length; i++) normals[i] = new Vector3(0, 0, 1);

      if (mesh == null){
          mesh = new Mesh();
      }
      mesh.vertices = newVerts.ToArray();
      mesh.triangles = triangles.ToArray().Reverse().ToArray(); // triangles need to be reversed so the spirals face the right way (I generated them backwards above, on accident)
      mesh.uv = uvs;
      mesh.uv2 = uv2s;
      mesh.normals = normals;
      mesh.RecalculateBounds();
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
}