/*
*** GENERATED CODE: ANY EDITS WILL BE LOST ***
*/
namespace PolymorphicStructsTests
{
	
	public partial struct ParameterMethodA
	{
		
		public ParameterMethodA(ParameterMethodInterface s)
		{
		}
		
		public void ToParameterMethodInterface(ref ParameterMethodInterface s)
		{
			s.CurrentTypeId = ParameterMethodInterface.TypeId.ParameterMethodA;
		}
		
		public ParameterMethodInterface ToParameterMethodInterface()
		{
			var s = new ParameterMethodInterface();
			ToParameterMethodInterface(ref s);
			return s;
		}
	}
}
