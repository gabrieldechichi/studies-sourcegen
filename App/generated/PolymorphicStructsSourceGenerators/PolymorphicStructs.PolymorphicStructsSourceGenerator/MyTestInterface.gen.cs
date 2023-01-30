/*
*** GENERATED CODE: ANY EDITS WILL BE LOST ***
*/

using System.Diagnostics;
using System;
using ConsoleApp;
namespace ConsoleApp
{
	[Serializable]
	public struct MyTestInterface : IMyTestInterface
	{
		public enum TypeId
		{
			MyStructA,
			MyStructB,
		}
		public TypeId CurrentTypeId;
		public Int32 Int32_0;
		public Int32 Int32_1;
		public Boolean Boolean_2;
		public void Foo()
		{
			switch (CurrentTypeId)
			{
				case TypeId.MyStructA:
				{
					var instance_MyStructA = new MyStructA(this);
					instance_MyStructA.Foo();
					instance_MyStructA.ToMyTestInterface(ref this);
					break;
				}
				case TypeId.MyStructB:
				{
					var instance_MyStructB = new MyStructB(this);
					instance_MyStructB.Foo();
					instance_MyStructB.ToMyTestInterface(ref this);
					break;
				}
				default:
				{
					break;
				}
			}
		}
		public int FooWithRetVal()
		{
			switch (CurrentTypeId)
			{
				case TypeId.MyStructA:
				{
					var instance_MyStructA = new MyStructA(this);
					var r = instance_MyStructA.FooWithRetVal();
					instance_MyStructA.ToMyTestInterface(ref this);
					return r;
				}
				case TypeId.MyStructB:
				{
					var instance_MyStructB = new MyStructB(this);
					var r = instance_MyStructB.FooWithRetVal();
					instance_MyStructB.ToMyTestInterface(ref this);
					return r;
				}
				default:
				{
					return default;
				}
			}
		}
	}
}
