/*
*** GENERATED CODE: ANY EDITS WILL BE LOST ***
*/

using NUnit.Framework.Interfaces;
using System;
using PolymorphicStructsTests;
namespace PolymorphicStructsTests
{
	[Serializable]
	public struct CorrectImplementationInterface : ICorrectImplementationInterface
	{
		public enum TypeId
		{
			CorrectImplementationA,
			CorrectImplementationB,
		}
		public TypeId CurrentTypeId;
		public Int32 Int32_0;
		public Int32 Int32_1;
		public int Foo()
		{
			switch (CurrentTypeId)
			{
				case TypeId.CorrectImplementationA:
				{
					var instance_CorrectImplementationA = new CorrectImplementationA(this);
					var r = instance_CorrectImplementationA.Foo();
					instance_CorrectImplementationA.ToCorrectImplementationInterface(ref this);
					return r;
				}
				case TypeId.CorrectImplementationB:
				{
					var instance_CorrectImplementationB = new CorrectImplementationB(this);
					var r = instance_CorrectImplementationB.Foo();
					instance_CorrectImplementationB.ToCorrectImplementationInterface(ref this);
					return r;
				}
				default:
				{
					throw new System.ArgumentOutOfRangeException($"Unexpected type id {CurrentTypeId} for merged struct CorrectImplementationInterface");
				}
			}
		}
	}
}
