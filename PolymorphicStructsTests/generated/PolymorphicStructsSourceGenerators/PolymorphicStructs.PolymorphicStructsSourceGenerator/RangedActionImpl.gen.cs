/*
*** GENERATED CODE: ANY EDITS WILL BE LOST ***
*/

namespace PolymorphicStructsTests
{
	public partial struct RangedActionImpl
	{
		public RangedActionImpl(RangedAction s)
		{
		}
		public void ToRangedAction(ref RangedAction s)
		{
			s.CurrentTypeId = RangedAction.TypeId.RangedActionImpl;
		}
		public RangedAction ToRangedAction()
		{
			var s = new RangedAction();
			ToRangedAction(ref s);
			return s;
		}
	}
}
