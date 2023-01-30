/*
*** GENERATED CODE: ANY EDITS WILL BE LOST ***
*/

namespace PolymorphicStructsTests
{
	public partial struct MeleeActionImpl
	{
		public MeleeActionImpl(MeleeAction s)
		{
		}
		public void ToMeleeAction(ref MeleeAction s)
		{
			s.CurrentTypeId = MeleeAction.TypeId.MeleeActionImpl;
		}
		public MeleeAction ToMeleeAction()
		{
			var s = new MeleeAction();
			ToMeleeAction(ref s);
			return s;
		}
	}
}
