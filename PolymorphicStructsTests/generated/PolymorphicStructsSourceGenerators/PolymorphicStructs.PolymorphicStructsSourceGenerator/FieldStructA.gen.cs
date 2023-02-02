/*
*** GENERATED CODE: ANY EDITS WILL BE LOST ***
*/
namespace PolymorphicStructsTests
{
	
	public partial struct FieldStructA
	{
		
		public FieldStructA(FieldInterface s)
		{
			Int1 = s.Int32_0;
			Int2 = s.Int32_1;
		}
		
		public void ToFieldInterface(ref FieldInterface s)
		{
			s.CurrentTypeId = FieldInterface.TypeId.FieldStructA;
			s.Int32_0 = Int1;
			s.Int32_1 = Int2;
		}
		
		public FieldInterface ToFieldInterface()
		{
			var s = new FieldInterface();
			ToFieldInterface(ref s);
			return s;
		}
	}
}
