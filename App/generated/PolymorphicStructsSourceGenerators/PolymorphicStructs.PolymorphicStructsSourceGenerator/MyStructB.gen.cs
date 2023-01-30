namespace ConsoleApp
{
	public partial struct MyStructB
	{
		public MyStructB(MyTestInterface s)
		{
			Field = s.Int32_0;
			OtherField = s.Boolean_2;
		}
		public void ToMyTestInterface(ref MyTestInterface s)
		{
			s.CurrentTypeId = MyTestInterface.TypeId.MyStructB;
			s.Int32_0 = Field;
			s.Boolean_2 = OtherField;
		}
		public MyTestInterface ToMyTestInterface()
		{
			var s = new MyTestInterface();
			ToMyTestInterface(ref s);
			return s;
		}
	}
}
