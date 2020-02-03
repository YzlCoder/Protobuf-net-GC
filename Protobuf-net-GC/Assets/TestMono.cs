using ProtoBuf;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

[ProtoContract]
[ProtoInclude(101, typeof(RectShape))]
[ProtoInclude(102, typeof(CircleShape))]
[ProtoInclude(103, typeof(TriangleShape))]
public class BaseShape
{
}

[ProtoContract]
public class RectShape : BaseShape
{
    [ProtoMember(1)]
    public Vector2 position;

    [ProtoMember(2)]
    public Vector2 size;

    public override string ToString()
    {
        return string.Format("This is a Rect {0}, {1}", position, size);
    }
}

[ProtoContract]
public class CircleShape : BaseShape
{
    [ProtoMember(1)]
    public Vector2 position;

    [ProtoMember(2)]
    public float radius;

    public override string ToString()
    {
        return string.Format("This is a Circle {0}, {1}", position, radius);
    }
}

[ProtoContract]
public class TriangleShape : BaseShape
{
    [ProtoMember(1)]
    public Vector2 position1;

    [ProtoMember(2)]
    public Vector2 position2;

    [ProtoMember(3)]
    public Vector2 position3;

    public override string ToString()
    {
        return string.Format("This is a Triang {0}, {1}, {2}", position1, position2, position3);
    }
}

[ProtoContract]
public class ShapeContainer
{
    [ProtoMember(1)]
    public List<BaseShape> Shapes = new List<BaseShape>();
}


public class TestMono : ProtoSerializeBaseMono
{

    public ShapeContainer ShapeContainer = new ShapeContainer();

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.G))
        {
            RectShape rectShape = new RectShape();
            rectShape.position = new Vector3(UnityEngine.Random.Range(-100, 100), UnityEngine.Random.Range(-100, 100));
            rectShape.size = new Vector3(UnityEngine.Random.Range(1, 100), UnityEngine.Random.Range(1, 100));
            this.ShapeContainer.Shapes.Add(rectShape);
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            CircleShape circleShape = new CircleShape();
            circleShape.position = new Vector3(UnityEngine.Random.Range(-100, 100), UnityEngine.Random.Range(-100, 100));
            circleShape.radius = UnityEngine.Random.Range(1, 100);
            this.ShapeContainer.Shapes.Add(circleShape);
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            TriangleShape triangleShape = new TriangleShape();
            triangleShape.position1 = new Vector3(UnityEngine.Random.Range(-100, 100), UnityEngine.Random.Range(-100, 100));
            triangleShape.position2 = new Vector3(UnityEngine.Random.Range(-100, 100), UnityEngine.Random.Range(-100, 100));
            triangleShape.position3 = new Vector3(UnityEngine.Random.Range(-100, 100), UnityEngine.Random.Range(-100, 100));
            this.ShapeContainer.Shapes.Add(triangleShape);
        }
    }

    private void OnGUI()
    {
        Rect position = new Rect(50, 30, 1000, 25);
        
        GUI.Label(position, string.Format("There are {0} shape", this.ShapeContainer.Shapes.Count));

        for(int i = 0; i < this.ShapeContainer.Shapes.Count; ++i)
        {
            position.y += 25;
            GUI.Label(position, string.Format("There are {0} shape", this.ShapeContainer.Shapes[i].ToString()));
        }


    }


    protected override void DeserializeObjects()
    {
        this.ShapeContainer = this.DeserializeObject<ShapeContainer>(this.ShapeContainer);
    }

    protected override void SerializeObjects()
    {
        this.SerializeObject<ShapeContainer>(this.ShapeContainer);
    }
}