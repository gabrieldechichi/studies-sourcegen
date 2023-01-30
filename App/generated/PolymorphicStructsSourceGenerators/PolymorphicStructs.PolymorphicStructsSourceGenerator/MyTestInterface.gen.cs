using System.Diagnostics;
using System;
using ConsoleApp;
namespace ConsoleApp
{
	[Serializable]
	public struct MyTestInterface
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
			throw new Exception();
		}

	}

}

