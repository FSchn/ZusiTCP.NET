﻿using System;

namespace ZusiTcpInterface.TypeDescriptors
{
  public class AttributeOrNodeDescriptor : IEquatable<AttributeOrNodeDescriptor>
  {
    private readonly string _unit;
    private readonly string _type;
    private readonly CabInfoAddress _address;
    private readonly string _qualifiedName;
    private readonly string _name;
    private readonly string _comment;
    private readonly bool _isNode;

    public AttributeOrNodeDescriptor(CabInfoAddress address, bool isNode, string qualifiedName, string name, string unit, string type, string comment = "")
    {
      if (address == null)
        throw new ArgumentNullException("address");
      if (qualifiedName == null)
        throw new ArgumentNullException("qualifiedName");
      if (name == null)
        throw new ArgumentNullException("name");
      if (unit == null) throw new ArgumentNullException("unit");
      if (type == null) throw new ArgumentNullException("type");
      if (comment == null)
        throw new ArgumentNullException("comment");

      _address = address;
      _qualifiedName = qualifiedName;
      _name = name;
      _unit = unit;
      _type = type;
      _comment = comment;
      _isNode = isNode;
    }

    public string Unit
    {
      get { return _unit; }
    }

    public string Type
    {
      get { return _type; }
    }

    public CabInfoAddress Address
    {
      get { return _address; }
    }

    public string Name
    {
      get { return _name; }
    }

    public string Comment
    {
      get { return _comment; }
    }

    public string QualifiedName
    {
      get { return _qualifiedName; }
    }
    
    public bool IsNode
    {
      get { return _isNode; }
    }

    #region Equality operations

    public bool Equals(AttributeOrNodeDescriptor other)
    {
      if (ReferenceEquals(null, other)) return false;
      if (ReferenceEquals(this, other)) return true;
      return _address == other._address
          && string.Equals(_qualifiedName, other._qualifiedName)
          && string.Equals(_name, other._name)
          && string.Equals(_comment, other._comment)
          && string.Equals(_unit, other._unit)
          && string.Equals(_type, other._type)
          && string.Equals(_isNode, other._isNode);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj)) return false;
      if (ReferenceEquals(this, obj)) return true;
      if (obj.GetType() != this.GetType()) return false;
      return Equals((AttributeOrNodeDescriptor) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = _address.GetHashCode();
        hashCode = (hashCode * 397) ^ _qualifiedName.GetHashCode();
        hashCode = (hashCode * 397) ^ _name.GetHashCode();
        hashCode = (hashCode * 397) ^ _comment.GetHashCode();
        hashCode = (hashCode * 397) ^ _unit.GetHashCode();
        hashCode = (hashCode * 397) ^ _type.GetHashCode();
        //hashCode = (hashCode * 397) ^ _isNode.GetHashCode(); //Nodes and Attributes usually won't collide...
        return hashCode;
      }
    }

    public static bool operator ==(AttributeOrNodeDescriptor left, AttributeOrNodeDescriptor right)
    {
      return Equals(left, right);
    }

    public static bool operator !=(AttributeOrNodeDescriptor left, AttributeOrNodeDescriptor right)
    {
      return !Equals(left, right);
    }

    #endregion Equality operations
  }
}