/*
*** GENERATED CODE: ANY EDITS WILL BE LOST ***
*/

using NUnit.Framework.Interfaces;
using System;
using PolymorphicStructsTests;
namespace PolymorphicStructsTests
{
	[Serializable]
	public struct ParameterMethodInterface : IParameterMethodInterface
	{
		public enum TypeId
		{
			ParameterMethodA,
			ParameterMethodB,
		}
		public TypeId CurrentTypeId;
		public void Foo(ref int a, in bool b, out int c, string s)
		{
			switch (CurrentTypeId)
			{
				case TypeId.ParameterMethodA:
				{
					var instance_ParameterMethodA = new ParameterMethodA(this);
					instance_ParameterMethodA.Foo(ref a, in b, out c, s);
					instance_ParameterMethodA.ToParameterMethodInterface(ref this);
					break;
				}
				case TypeId.ParameterMethodB:
				{
					var instance_ParameterMethodB = new ParameterMethodB(this);
					instance_ParameterMethodB.Foo(ref a, in b, out c, s);
					instance_ParameterMethodB.ToParameterMethodInterface(ref this);
					break;
				}
				default:
				{
					throw new System.ArgumentOutOfRangeException($"Unexpected type id {CurrentTypeId} for merged struct ParameterMethodInterface");
				}
			}
		}
	}
}
