using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class RopeControllerSimple : MonoBehaviour
{
    //Objects that will interact with the rope
    public Transform whatTheRopeIsConnectedTo;
    public Transform whatIsHangingFromTheRope;

    //Line renderer used to display the rope
    private LineRenderer lineRenderer;

    //Rope data
    [SerializeField] private float ropeLength = 1f;
    [SerializeField] private float minRopeLength = 1f;
    [SerializeField] private float maxRopeLength = 20f;

    //Mass of what the rope is carrying
    private float loadMass = 100f;
    //How fast we can add more/less rope
    float winchSpeed = 2f;

    private List<RopeSegment> ropeSegments = new List<RopeSegment>();
    [SerializeField] private float ropeSegmentLength = 0.2f;
    private int segmentCount = 20;
    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private int startSegmentCount = 10;

    //The joint we use to approximate the rope
    SpringJoint springJoint;

    [SerializeField] float springPower = .5f;
    [SerializeField] float springDamper = .5f;

    void Start()
    {
        springJoint = whatTheRopeIsConnectedTo.GetComponent<SpringJoint>();

        lineRenderer = GetComponentInChildren<LineRenderer>();

        Vector3 ropeStartPoint = Vector3.zero;
        segmentCount = startSegmentCount;
        for (int i = 0; i < segmentCount; i++)
        {
            ropeSegments.Add(new RopeSegment(ropeStartPoint));
            ropeStartPoint.y += ropeSegmentLength;
        }

        UpdateSpring();

        //Add the weight to what the rope is carrying
        whatIsHangingFromTheRope.GetComponent<Rigidbody>().mass = loadMass;
    }

    void Update()
    {
        //Display the rope with a line renderer
        DisplayRope();

        
    }

    private void FixedUpdate()
    {
        UpdateWinch();
        Simulation();
    }

    private void InitRope()
    {
        float dist = ropeLength;

        int tempSegmentCount = (int)(dist * 2f) + startSegmentCount;

        if (tempSegmentCount > ropeSegments.Count)
        {
            Vector3 ropeStartPoint = ropeSegments[ropeSegments.Count - 1].posNow;
            segmentCount = tempSegmentCount;
            ropeStartPoint.y += ropeSegmentLength;
            ropeSegments.Add(new RopeSegment(ropeStartPoint));
        }
        else if (tempSegmentCount < ropeSegments.Count)
        {
            segmentCount = tempSegmentCount;
            ropeSegments.RemoveAt(ropeSegments.Count - 1);
        }
    }

    private void Simulation()
    {
        Vector3 forceGravity = new Vector3(0f, -1f, 0f);

        for (int i = 1; i < ropeSegments.Count; i++)
        {
            RopeSegment currentSegment = ropeSegments[i];
            Vector3 velocity = currentSegment.posNow - currentSegment.posOld;
            currentSegment.posOld = currentSegment.posNow;
            currentSegment.posNow += velocity;
            currentSegment.posNow += forceGravity * Time.fixedDeltaTime;
            ropeSegments[i] = currentSegment;
        }

        for (int i =0; i < 20; i++)
        {
            ApplyConstraint();
        }
    }

    private void ApplyConstraint()
    {
        RopeSegment firstSegment = ropeSegments[0];
        firstSegment.posNow = whatTheRopeIsConnectedTo.position;
        ropeSegments[0] = firstSegment;

        RopeSegment endSegment = ropeSegments[ropeSegments.Count - 1];
        endSegment.posNow = whatIsHangingFromTheRope.position;
        ropeSegments[ropeSegments.Count - 1] = endSegment;

        for (int i = 0; i < ropeSegments.Count - 1; i++)
        {
            RopeSegment firstSeg = ropeSegments[i];
            RopeSegment secondSeg = ropeSegments[i + 1];


            float dist = (firstSeg.posNow - secondSeg.posNow).magnitude;
            float error = Mathf.Abs(dist - ropeSegmentLength);
            Vector3 changeDir = Vector3.zero;

            if (dist > ropeSegmentLength)
            {
                changeDir = (firstSeg.posNow - secondSeg.posNow).normalized;
            }
            else if (dist < ropeSegmentLength)
            {
                changeDir = (secondSeg.posNow - firstSeg.posNow).normalized;
            }

            Vector3 changeAmount = changeDir * error;
            
            if (i != 0)
            {
                firstSeg.posNow -= changeAmount * .5f;
                ropeSegments[i] = firstSeg;
                secondSeg.posNow += changeAmount * .5f;
                ropeSegments[i+1] = secondSeg;
            }
            else
            {
                secondSeg.posNow += changeAmount;
                ropeSegments[i + 1] = secondSeg;
            }
        }
    }

    //Update the spring constant and the length of the spring
    private void UpdateSpring()
    {
        float density = 7750f;
        float radius = 0.02f;
        float volume = Mathf.PI * radius * radius * ropeLength;
        float ropeMass = volume * density;

        //Add what the rope is carrying
        ropeMass += loadMass;

        float ropeForce = ropeMass * 9.81f;
        float kRope = ropeForce / 0.01f;

        //Add the value to the spring
        springJoint.spring = kRope * springPower;
        springJoint.damper = kRope * springDamper;

        //Update length of the rope
        springJoint.maxDistance = ropeLength;
    }

    private void DisplayRope()
    {
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        lineRenderer.positionCount = ropeSegments.Count;

        Vector3[] ropePosition = new Vector3[ropeSegments.Count];
        for (int i = 0; i < ropeSegments.Count; i++)
        {
            ropePosition[i] = ropeSegments[i].posNow;
        }

        ropePosition[0] = whatTheRopeIsConnectedTo.position;
        ropePosition[ropePosition.Length - 1] = whatIsHangingFromTheRope.position;

        lineRenderer.SetPositions(ropePosition);
    }

    //Add more/less rope
    private void UpdateWinch()
    {
        bool hasChangedRope = false;

        if (Keyboard.current.oKey.isPressed && ropeLength < maxRopeLength)
        {
            ropeLength += winchSpeed * Time.deltaTime;

            InitRope();

            whatIsHangingFromTheRope.gameObject.GetComponent<Rigidbody>().WakeUp();

            hasChangedRope = true;
        }
        else if (Keyboard.current.iKey.isPressed && ropeLength > minRopeLength)
        {
            ropeLength -= winchSpeed * Time.deltaTime;

            InitRope();
            whatIsHangingFromTheRope.gameObject.GetComponent<Rigidbody>().WakeUp();

            hasChangedRope = true;
        }

        if (hasChangedRope)
        {
            ropeLength = Mathf.Clamp(ropeLength, minRopeLength, maxRopeLength);
            UpdateSpring();
        }
    }

    public struct RopeSegment
    {
        public Vector3 posNow;
        public Vector3 posOld;

        public RopeSegment(Vector3 pos)
        {
            posNow = pos;
            posOld = pos;
        }
    }
}
