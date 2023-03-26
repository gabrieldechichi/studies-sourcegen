/*
*** GENERATED CODE: ANY EDITS WILL BE LOST ***
*/
namespace PolymorphicStructsTests
{
	
	public partial struct CorrectImplementationB
	{
		
		public CorrectImplementationB(CorrectImplementationInterface s)
		{
			A = s.Int32_0;
			B = s.Int32_1;
		}
		
		public void ToCorrectImplementationInterface(ref CorrectImplementationInterface s)
		{
			s.CurrentTypeId = CorrectImplementationInterface.TypeId.CorrectImplementationB;
			s.Int32_0 = A;
			s.Int32_1 = B;
		}
		
		public CorrectImplementationInterface ToCorrectImplementationInterface()
		{
			var s = new CorrectImplementationInterface();
			ToCorrectImplementationInterface(ref s);
			return s;
		}
	}
}
