/*
*** GENERATED CODE: ANY EDITS WILL BE LOST ***
*/
using NUnit.Framework.Interfaces;
using System;
using PolymorphicStructsTests;

namespace PolymorphicStructsTests
{
	[Serializable]
	
	public struct RangedAction : IRangedAction
	{
		
		public enum TypeId
		{
			RangedActionImpl,
		}
		
		public TypeId CurrentTypeId;
		
		public int PerformAction()
		{
			switch (CurrentTypeId)
			{
				case TypeId.RangedActionImpl:
				{
					var instance_RangedActionImpl = new RangedActionImpl(this);
					var r = instance_RangedActionImpl.PerformAction();
					instance_RangedActionImpl.ToRangedAction(ref this);
					return r;
				}
				default:
				{
					throw new System.ArgumentOutOfRangeException($"Unexpected type id {CurrentTypeId} for merged struct RangedAction");
				}
			}
		}
	}
}
