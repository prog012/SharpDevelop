﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under MIT X11 license (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;

using ICSharpCode.NRefactory.TypeSystem.Implementation;

namespace ICSharpCode.NRefactory.TypeSystem
{
	/// <summary>
	/// Represents the intersection of several types.
	/// </summary>
	public class IntersectionType : AbstractType
	{
		readonly ReadOnlyCollection<IType> types;
		
		public ReadOnlyCollection<IType> Types {
			get { return types; }
		}
		
		private IntersectionType(IType[] types)
		{
			Debug.Assert(types.Length >= 2);
			this.types = Array.AsReadOnly(types);
		}
		
		public static IType Create(IEnumerable<IType> types)
		{
			IType[] arr = types.Where(t => t != null).Distinct().ToArray();
			if (arr.Length == 0)
				return SharedTypes.UnknownType;
			else if (arr.Length == 1)
				return arr[0];
			else
				return new IntersectionType(arr);
		}
		
		public override string Name {
			get {
				StringBuilder b = new StringBuilder();
				foreach (var t in types) {
					if (b.Length > 0)
						b.Append(" & ");
					b.Append(t.Name);
				}
				return b.ToString();
			}
		}
		
		public override string ReflectionName {
			get {
				StringBuilder b = new StringBuilder();
				foreach (var t in types) {
					if (b.Length > 0)
						b.Append(" & ");
					b.Append(t.ReflectionName);
				}
				return b.ToString();
			}
		}
		
		public override bool? IsReferenceType(ITypeResolveContext context)
		{
			foreach (var t in types) {
				bool? isReferenceType = t.IsReferenceType(context);
				if (isReferenceType.HasValue)
					return isReferenceType.Value;
			}
			return null;
		}
		
		public override int GetHashCode()
		{
			int hashCode = 0;
			unchecked {
				foreach (var t in types) {
					hashCode *= 7137517;
					hashCode += t.GetHashCode();
				}
			}
			return hashCode;
		}
		
		public override bool Equals(IType other)
		{
			IntersectionType o = other as IntersectionType;
			if (o != null && types.Count == o.types.Count) {
				for (int i = 0; i < types.Count; i++) {
					if (!types[i].Equals(o.types[i]))
						return false;
				}
				return true;
			}
			return false;
		}
		
		public override IEnumerable<IType> GetBaseTypes(ITypeResolveContext context)
		{
			return types;
		}
		
		public override IEnumerable<IMethod> GetMethods(ITypeResolveContext context, Predicate<IMethod> filter)
		{
			return ParameterizedType.GetMethods(this, context, FilterNonStatic(filter));
		}
		
		public override IEnumerable<IProperty> GetProperties(ITypeResolveContext context, Predicate<IProperty> filter)
		{
			return ParameterizedType.GetProperties(this, context, FilterNonStatic(filter));
		}
		
		public override IEnumerable<IField> GetFields(ITypeResolveContext context, Predicate<IField> filter)
		{
			return ParameterizedType.GetFields(this, context, FilterNonStatic(filter));
		}
		
		public override IEnumerable<IEvent> GetEvents(ITypeResolveContext context, Predicate<IEvent> filter)
		{
			return ParameterizedType.GetEvents(this, context, FilterNonStatic(filter));
		}
		
		public override IEnumerable<IMember> GetMembers(ITypeResolveContext context, Predicate<IMember> filter)
		{
			return ParameterizedType.GetMembers(this, context, FilterNonStatic(filter));
		}
		
		static Predicate<T> FilterNonStatic<T>(Predicate<T> filter) where T : class, IMember
		{
			if (filter == null)
				return member => !member.IsStatic;
			else
				return member => !member.IsStatic && filter(member);
		}
	}
}
