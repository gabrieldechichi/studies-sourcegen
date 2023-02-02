/*
*** GENERATED CODE: ANY EDITS WILL BE LOST ***
*/
namespace PolymorphicStructsTests
{
	
	public partial struct FieldStructB
	{
		
		public FieldStructB(FieldInterface s)
		{
			Int = s.Int32_0;
		}
		
		public void ToFieldInterface(ref FieldInterface s)
		{
			s.CurrentTypeId = FieldInterface.TypeId.FieldStructB;
			s.Int32_0 = Int;
		}
		
		public FieldInterface ToFieldInterface()
		{
			var s = new FieldInterface();
			ToFieldInterface(ref s);
			return s;
		}
	}
}
