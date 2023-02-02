/*
*** GENERATED CODE: ANY EDITS WILL BE LOST ***
*/
using NUnit.Framework.Interfaces;
using System;
using PolymorphicStructsTests;

namespace PolymorphicStructsTests
{
	[Serializable]
	
	public struct VoidMethodInterface : IVoidMethodInterface
	{
		
		public enum TypeId
		{
			VoidMethodA,
			VoidMethodB,
		}
		
		public TypeId CurrentTypeId;
		public Int32 Int32_0;
		
		public void Foo()
		{
			switch (CurrentTypeId)
			{
				case TypeId.VoidMethodA:
				{
					var instance_VoidMethodA = new VoidMethodA(this);
					instance_VoidMethodA.Foo();
					instance_VoidMethodA.ToVoidMethodInterface(ref this);
					break;
				}
				case TypeId.VoidMethodB:
				{
					var instance_VoidMethodB = new VoidMethodB(this);
					instance_VoidMethodB.Foo();
					instance_VoidMethodB.ToVoidMethodInterface(ref this);
					break;
				}
				default:
				{
					throw new System.ArgumentOutOfRangeException($"Unexpected type id {CurrentTypeId} for merged struct VoidMethodInterface");
				}
			}
		}
	}
}
