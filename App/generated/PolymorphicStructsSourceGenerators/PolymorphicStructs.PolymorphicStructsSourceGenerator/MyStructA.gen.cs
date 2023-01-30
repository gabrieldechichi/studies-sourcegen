namespace ConsoleApp
{
	public partial struct MyStructA
	{
		public MyStructA(MyTestInterface s)
		{
			Field1 = s.Int32_0;
			Field2 = s.Int32_1;
		}

		public void ToMyTestInterface(ref MyTestInterface s)
		{
			s.CurrentTypeId = MyTestInterface.TypeId.MyStructA;
			s.Int32_0 = Field1;
			s.Int32_1 = Field2;
		}

		public MyTestInterface ToMyTestInterface()
		{
			var s = new MyTestInterface();
			ToMyTestInterface(ref s);
			return s;
		}

	}

}

