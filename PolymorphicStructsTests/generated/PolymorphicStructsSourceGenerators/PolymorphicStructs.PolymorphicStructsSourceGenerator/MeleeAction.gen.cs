/*
*** GENERATED CODE: ANY EDITS WILL BE LOST ***
*/

using NUnit.Framework.Interfaces;
using System;
using PolymorphicStructsTests;
namespace PolymorphicStructsTests
{
	[Serializable]
	public struct MeleeAction : IMeleeAction
	{
		public enum TypeId
		{
			MeleeActionImpl,
		}
		public TypeId CurrentTypeId;
		public int PerformAction()
		{
			switch (CurrentTypeId)
			{
				case TypeId.MeleeActionImpl:
				{
					var instance_MeleeActionImpl = new MeleeActionImpl(this);
					var r = instance_MeleeActionImpl.PerformAction();
					instance_MeleeActionImpl.ToMeleeAction(ref this);
					return r;
				}
				default:
				{
					throw new System.ArgumentOutOfRangeException($"Unexpected type id {CurrentTypeId} for merged struct MeleeAction");
				}
			}
		}
	}
}
