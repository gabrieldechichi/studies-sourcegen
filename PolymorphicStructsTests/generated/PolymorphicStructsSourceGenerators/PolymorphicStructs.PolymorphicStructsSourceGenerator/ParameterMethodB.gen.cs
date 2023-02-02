/*
*** GENERATED CODE: ANY EDITS WILL BE LOST ***
*/
namespace PolymorphicStructsTests
{
	
	public partial struct ParameterMethodB
	{
		
		public ParameterMethodB(ParameterMethodInterface s)
		{
		}
		
		public void ToParameterMethodInterface(ref ParameterMethodInterface s)
		{
			s.CurrentTypeId = ParameterMethodInterface.TypeId.ParameterMethodB;
		}
		
		public ParameterMethodInterface ToParameterMethodInterface()
		{
			var s = new ParameterMethodInterface();
			ToParameterMethodInterface(ref s);
			return s;
		}
	}
}
