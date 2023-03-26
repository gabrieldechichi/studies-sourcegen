/*
*** GENERATED CODE: ANY EDITS WILL BE LOST ***
*/
namespace PolymorphicStructsTests
{
	
	public partial struct FieldStructC
	{
		
		public FieldStructC(FieldInterface s)
		{
			Bool = s.Boolean_2;
			Int = s.Int32_0;
		}
		
		public void ToFieldInterface(ref FieldInterface s)
		{
			s.CurrentTypeId = FieldInterface.TypeId.FieldStructC;
			s.Int32_0 = Int;
			s.Boolean_2 = Bool;
		}
		
		public FieldInterface ToFieldInterface()
		{
			var s = new FieldInterface();
			ToFieldInterface(ref s);
			return s;
		}
	}
}
