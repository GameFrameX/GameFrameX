using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"GameFrameX.Asset.Runtime.dll",
		"GameFrameX.Config.Runtime.dll",
		"GameFrameX.FairyGUI.Runtime.dll",
		"GameFrameX.Network.Runtime.dll",
		"LuBan.Runtime.dll",
		"System.Core.dll",
		"System.dll",
		"UniTask.Runtime.dll",
		"UnityEngine.CoreModule.dll",
		"YooAsset.Runtime.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// Cysharp.Threading.Tasks.IUniTaskSource<System.ValueTuple<byte,object>>
	// Cysharp.Threading.Tasks.IUniTaskSource<object>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<System.ValueTuple<byte,object>>
	// Cysharp.Threading.Tasks.UniTask.Awaiter<object>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<System.ValueTuple<byte,object>>
	// Cysharp.Threading.Tasks.UniTask.IsCanceledSource<object>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<System.ValueTuple<byte,object>>
	// Cysharp.Threading.Tasks.UniTask.MemoizeSource<object>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// Cysharp.Threading.Tasks.UniTask<System.ValueTuple<byte,object>>
	// Cysharp.Threading.Tasks.UniTask<object>
	// Cysharp.Threading.Tasks.UniTaskCompletionSource<object>
	// GameFrameX.FairyGUI.Runtime.FairyGUIComponent.<>c__DisplayClass18_0<object>
	// System.Action<Hotfix.Config.test.DemoE2>
	// System.Action<System.ValueTuple<object,object>>
	// System.Action<UnityEngine.Vector3>
	// System.Action<UnityEngine.Vector4>
	// System.Action<byte,object,object>
	// System.Action<int>
	// System.Action<long>
	// System.Action<object,object>
	// System.Action<object>
	// System.Collections.Generic.ArraySortHelper<Hotfix.Config.test.DemoE2>
	// System.Collections.Generic.ArraySortHelper<System.ValueTuple<object,object>>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.Vector3>
	// System.Collections.Generic.ArraySortHelper<UnityEngine.Vector4>
	// System.Collections.Generic.ArraySortHelper<int>
	// System.Collections.Generic.ArraySortHelper<long>
	// System.Collections.Generic.ArraySortHelper<object>
	// System.Collections.Generic.Comparer<Hotfix.Config.test.DemoE2>
	// System.Collections.Generic.Comparer<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.Comparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Collections.Generic.Comparer<System.ValueTuple<byte,object>>
	// System.Collections.Generic.Comparer<System.ValueTuple<object,object>>
	// System.Collections.Generic.Comparer<UnityEngine.Vector3>
	// System.Collections.Generic.Comparer<UnityEngine.Vector4>
	// System.Collections.Generic.Comparer<byte>
	// System.Collections.Generic.Comparer<int>
	// System.Collections.Generic.Comparer<long>
	// System.Collections.Generic.Comparer<object>
	// System.Collections.Generic.Dictionary.Enumerator<System.ValueTuple<int,long,object>,object>
	// System.Collections.Generic.Dictionary.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.Enumerator<long,int>
	// System.Collections.Generic.Dictionary.Enumerator<long,object>
	// System.Collections.Generic.Dictionary.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<System.ValueTuple<int,long,object>,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<long,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<long,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection<System.ValueTuple<int,long,object>,object>
	// System.Collections.Generic.Dictionary.KeyCollection<int,int>
	// System.Collections.Generic.Dictionary.KeyCollection<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection<long,int>
	// System.Collections.Generic.Dictionary.KeyCollection<long,object>
	// System.Collections.Generic.Dictionary.KeyCollection<object,int>
	// System.Collections.Generic.Dictionary.KeyCollection<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<System.ValueTuple<int,long,object>,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<long,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<long,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,int>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<System.ValueTuple<int,long,object>,object>
	// System.Collections.Generic.Dictionary.ValueCollection<int,int>
	// System.Collections.Generic.Dictionary.ValueCollection<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection<long,int>
	// System.Collections.Generic.Dictionary.ValueCollection<long,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,int>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary<System.ValueTuple<int,long,object>,object>
	// System.Collections.Generic.Dictionary<int,int>
	// System.Collections.Generic.Dictionary<int,object>
	// System.Collections.Generic.Dictionary<long,int>
	// System.Collections.Generic.Dictionary<long,object>
	// System.Collections.Generic.Dictionary<object,int>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.EqualityComparer<Hotfix.Config.test.DemoE2>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<byte,object>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<int,long,object>>
	// System.Collections.Generic.EqualityComparer<System.ValueTuple<object,object>>
	// System.Collections.Generic.EqualityComparer<UnityEngine.Vector3>
	// System.Collections.Generic.EqualityComparer<UnityEngine.Vector4>
	// System.Collections.Generic.EqualityComparer<byte>
	// System.Collections.Generic.EqualityComparer<int>
	// System.Collections.Generic.EqualityComparer<long>
	// System.Collections.Generic.EqualityComparer<object>
	// System.Collections.Generic.HashSet.Enumerator<int>
	// System.Collections.Generic.HashSet.Enumerator<object>
	// System.Collections.Generic.HashSet<int>
	// System.Collections.Generic.HashSet<object>
	// System.Collections.Generic.HashSetEqualityComparer<int>
	// System.Collections.Generic.HashSetEqualityComparer<object>
	// System.Collections.Generic.ICollection<Hotfix.Config.test.DemoE2>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<System.ValueTuple<int,long,object>,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<long,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ICollection<System.ValueTuple<object,object>>
	// System.Collections.Generic.ICollection<UnityEngine.Vector3>
	// System.Collections.Generic.ICollection<UnityEngine.Vector4>
	// System.Collections.Generic.ICollection<int>
	// System.Collections.Generic.ICollection<long>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.IComparer<Hotfix.Config.test.DemoE2>
	// System.Collections.Generic.IComparer<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.IComparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IComparer<System.ValueTuple<object,object>>
	// System.Collections.Generic.IComparer<UnityEngine.Vector3>
	// System.Collections.Generic.IComparer<UnityEngine.Vector4>
	// System.Collections.Generic.IComparer<int>
	// System.Collections.Generic.IComparer<long>
	// System.Collections.Generic.IComparer<object>
	// System.Collections.Generic.IEnumerable<Hotfix.Config.test.DemoE2>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<System.ValueTuple<int,long,object>,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<long,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerable<System.ValueTuple<object,object>>
	// System.Collections.Generic.IEnumerable<UnityEngine.Vector3>
	// System.Collections.Generic.IEnumerable<UnityEngine.Vector4>
	// System.Collections.Generic.IEnumerable<int>
	// System.Collections.Generic.IEnumerable<long>
	// System.Collections.Generic.IEnumerable<object>
	// System.Collections.Generic.IEnumerator<Hotfix.Config.test.DemoE2>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<System.ValueTuple<int,long,object>,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<long,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,int>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerator<System.ValueTuple<object,object>>
	// System.Collections.Generic.IEnumerator<UnityEngine.Vector3>
	// System.Collections.Generic.IEnumerator<UnityEngine.Vector4>
	// System.Collections.Generic.IEnumerator<int>
	// System.Collections.Generic.IEnumerator<long>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IEqualityComparer<System.ValueTuple<int,long,object>>
	// System.Collections.Generic.IEqualityComparer<int>
	// System.Collections.Generic.IEqualityComparer<long>
	// System.Collections.Generic.IEqualityComparer<object>
	// System.Collections.Generic.IList<Hotfix.Config.test.DemoE2>
	// System.Collections.Generic.IList<System.ValueTuple<object,object>>
	// System.Collections.Generic.IList<UnityEngine.Vector3>
	// System.Collections.Generic.IList<UnityEngine.Vector4>
	// System.Collections.Generic.IList<int>
	// System.Collections.Generic.IList<long>
	// System.Collections.Generic.IList<object>
	// System.Collections.Generic.KeyValuePair<System.ValueTuple<int,long,object>,object>
	// System.Collections.Generic.KeyValuePair<int,int>
	// System.Collections.Generic.KeyValuePair<int,object>
	// System.Collections.Generic.KeyValuePair<long,int>
	// System.Collections.Generic.KeyValuePair<long,object>
	// System.Collections.Generic.KeyValuePair<object,int>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.List.Enumerator<Hotfix.Config.test.DemoE2>
	// System.Collections.Generic.List.Enumerator<System.ValueTuple<object,object>>
	// System.Collections.Generic.List.Enumerator<UnityEngine.Vector3>
	// System.Collections.Generic.List.Enumerator<UnityEngine.Vector4>
	// System.Collections.Generic.List.Enumerator<int>
	// System.Collections.Generic.List.Enumerator<long>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List.SynchronizedList<Hotfix.Config.test.DemoE2>
	// System.Collections.Generic.List.SynchronizedList<System.ValueTuple<object,object>>
	// System.Collections.Generic.List.SynchronizedList<UnityEngine.Vector3>
	// System.Collections.Generic.List.SynchronizedList<UnityEngine.Vector4>
	// System.Collections.Generic.List.SynchronizedList<int>
	// System.Collections.Generic.List.SynchronizedList<long>
	// System.Collections.Generic.List.SynchronizedList<object>
	// System.Collections.Generic.List<Hotfix.Config.test.DemoE2>
	// System.Collections.Generic.List<System.ValueTuple<object,object>>
	// System.Collections.Generic.List<UnityEngine.Vector3>
	// System.Collections.Generic.List<UnityEngine.Vector4>
	// System.Collections.Generic.List<int>
	// System.Collections.Generic.List<long>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.ObjectComparer<Hotfix.Config.test.DemoE2>
	// System.Collections.Generic.ObjectComparer<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.ObjectComparer<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<byte,object>>
	// System.Collections.Generic.ObjectComparer<System.ValueTuple<object,object>>
	// System.Collections.Generic.ObjectComparer<UnityEngine.Vector3>
	// System.Collections.Generic.ObjectComparer<UnityEngine.Vector4>
	// System.Collections.Generic.ObjectComparer<byte>
	// System.Collections.Generic.ObjectComparer<int>
	// System.Collections.Generic.ObjectComparer<long>
	// System.Collections.Generic.ObjectComparer<object>
	// System.Collections.Generic.ObjectEqualityComparer<Hotfix.Config.test.DemoE2>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<byte,object>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<int,long,object>>
	// System.Collections.Generic.ObjectEqualityComparer<System.ValueTuple<object,object>>
	// System.Collections.Generic.ObjectEqualityComparer<UnityEngine.Vector3>
	// System.Collections.Generic.ObjectEqualityComparer<UnityEngine.Vector4>
	// System.Collections.Generic.ObjectEqualityComparer<byte>
	// System.Collections.Generic.ObjectEqualityComparer<int>
	// System.Collections.Generic.ObjectEqualityComparer<long>
	// System.Collections.Generic.ObjectEqualityComparer<object>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_0<long,object>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_0<object,object>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_1<long,object>
	// System.Collections.Generic.SortedDictionary.<>c__DisplayClass34_1<object,object>
	// System.Collections.Generic.SortedDictionary.Enumerator<long,object>
	// System.Collections.Generic.SortedDictionary.Enumerator<object,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass5_0<long,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass5_0<object,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass6_0<long,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass6_0<object,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass6_1<long,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.<>c__DisplayClass6_1<object,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.Enumerator<long,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection.Enumerator<object,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection<long,object>
	// System.Collections.Generic.SortedDictionary.KeyCollection<object,object>
	// System.Collections.Generic.SortedDictionary.KeyValuePairComparer<long,object>
	// System.Collections.Generic.SortedDictionary.KeyValuePairComparer<object,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass5_0<long,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass5_0<object,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass6_0<long,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass6_0<object,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass6_1<long,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.<>c__DisplayClass6_1<object,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.Enumerator<long,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection<long,object>
	// System.Collections.Generic.SortedDictionary.ValueCollection<object,object>
	// System.Collections.Generic.SortedDictionary<long,object>
	// System.Collections.Generic.SortedDictionary<object,object>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass52_0<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass52_0<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass53_0<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass53_0<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass53_1<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.SortedSet.<>c__DisplayClass53_1<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.SortedSet.Enumerator<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.SortedSet.Enumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.SortedSet.Node<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.SortedSet.Node<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.SortedSet<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.SortedSet<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.Stack.Enumerator<object>
	// System.Collections.Generic.Stack<object>
	// System.Collections.Generic.TreeSet<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.TreeSet<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.TreeWalkPredicate<System.Collections.Generic.KeyValuePair<long,object>>
	// System.Collections.Generic.TreeWalkPredicate<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<Hotfix.Config.test.DemoE2>
	// System.Collections.ObjectModel.ReadOnlyCollection<System.ValueTuple<object,object>>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.Vector3>
	// System.Collections.ObjectModel.ReadOnlyCollection<UnityEngine.Vector4>
	// System.Collections.ObjectModel.ReadOnlyCollection<int>
	// System.Collections.ObjectModel.ReadOnlyCollection<long>
	// System.Collections.ObjectModel.ReadOnlyCollection<object>
	// System.Comparison<Hotfix.Config.test.DemoE2>
	// System.Comparison<System.ValueTuple<object,object>>
	// System.Comparison<UnityEngine.Vector3>
	// System.Comparison<UnityEngine.Vector4>
	// System.Comparison<int>
	// System.Comparison<long>
	// System.Comparison<object>
	// System.EventHandler<object>
	// System.Func<System.Threading.Tasks.VoidTaskResult>
	// System.Func<object,System.Threading.Tasks.VoidTaskResult>
	// System.Func<object,object,object>
	// System.Func<object,object>
	// System.Func<object>
	// System.Nullable<UnityEngine.Vector3>
	// System.Nullable<int>
	// System.Predicate<Hotfix.Config.test.DemoE2>
	// System.Predicate<System.ValueTuple<object,object>>
	// System.Predicate<UnityEngine.Vector3>
	// System.Predicate<UnityEngine.Vector4>
	// System.Predicate<int>
	// System.Predicate<long>
	// System.Predicate<object>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<object>
	// System.Runtime.CompilerServices.TaskAwaiter<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.TaskAwaiter<object>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<object>
	// System.Threading.Tasks.Task.<>c<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.Task.<>c<object>
	// System.Threading.Tasks.Task<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.Task<object>
	// System.Threading.Tasks.TaskCompletionSource<object>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<object>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_1<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_1<object>
	// System.Threading.Tasks.TaskFactory<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.TaskFactory<object>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,System.ValueTuple<byte,object>>>
	// System.ValueTuple<byte,System.ValueTuple<byte,object>>
	// System.ValueTuple<byte,object>
	// System.ValueTuple<int,long,object>
	// System.ValueTuple<object,object>
	// }}

	public void RefMethods()
	{
		// Cysharp.Threading.Tasks.UniTask<YooAsset.AssetHandle> GameFrameX.Asset.Runtime.AssetComponent.LoadAssetAsync<object>(string)
		// Cysharp.Threading.Tasks.UniTask<YooAsset.AssetHandle> GameFrameX.Asset.Runtime.IAssetManager.LoadAssetAsync<object>(string)
		// object GameFrameX.Config.Runtime.ConfigComponent.GetConfig<object>()
		// bool GameFrameX.Config.Runtime.ConfigComponent.HasConfig<object>()
		// System.Threading.Tasks.Task<object> GameFrameX.FairyGUI.Runtime.FairyGUIComponent.AddAsync<object>(System.Func<object,object>,string,GameFrameX.FairyGUI.Runtime.UILayer,bool,object)
		// object GameFrameX.FairyGUI.Runtime.FairyGUIComponent.Get<object>(string)
		// object GameFrameX.FairyGUI.Runtime.GObjectHelper.Get<object>(FairyGUI.GObject)
		// System.Threading.Tasks.Task<object> GameFrameX.Network.Runtime.INetworkChannel.Call<object>(GameFrameX.Network.Runtime.MessageObject)
		// string LuBan.Runtime.StringUtil.CollectionToString<Hotfix.Config.test.DemoE2>(System.Collections.Generic.IEnumerable<Hotfix.Config.test.DemoE2>)
		// string LuBan.Runtime.StringUtil.CollectionToString<UnityEngine.Vector3>(System.Collections.Generic.IEnumerable<UnityEngine.Vector3>)
		// string LuBan.Runtime.StringUtil.CollectionToString<UnityEngine.Vector4>(System.Collections.Generic.IEnumerable<UnityEngine.Vector4>)
		// string LuBan.Runtime.StringUtil.CollectionToString<int,int>(System.Collections.Generic.IDictionary<int,int>)
		// string LuBan.Runtime.StringUtil.CollectionToString<int,object>(System.Collections.Generic.IDictionary<int,object>)
		// string LuBan.Runtime.StringUtil.CollectionToString<int>(System.Collections.Generic.IEnumerable<int>)
		// string LuBan.Runtime.StringUtil.CollectionToString<long,int>(System.Collections.Generic.IDictionary<long,int>)
		// string LuBan.Runtime.StringUtil.CollectionToString<long>(System.Collections.Generic.IEnumerable<long>)
		// string LuBan.Runtime.StringUtil.CollectionToString<object,int>(System.Collections.Generic.IDictionary<object,int>)
		// string LuBan.Runtime.StringUtil.CollectionToString<object>(System.Collections.Generic.IEnumerable<object>)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,Hotfix.Config.TablesComponent.<LoadAsync>d__162>(System.Runtime.CompilerServices.TaskAwaiter&,Hotfix.Config.TablesComponent.<LoadAsync>d__162&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.ai.TbBehaviorTree.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.ai.TbBehaviorTree.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.ai.TbBlackboard.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.ai.TbBlackboard.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.common.TbGlobalConfig.<LoadAsync>d__5>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.common.TbGlobalConfig.<LoadAsync>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.item.TbItem.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.item.TbItem.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.l10n.TbL10NDemo.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.l10n.TbL10NDemo.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.l10n.TbPatchDemo.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.l10n.TbPatchDemo.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.tag.TbTestTag.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.tag.TbTestTag.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbCompositeJsonTable1.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbCompositeJsonTable1.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbCompositeJsonTable2.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbCompositeJsonTable2.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbCompositeJsonTable3.<LoadAsync>d__5>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbCompositeJsonTable3.<LoadAsync>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbDataFromMisc.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbDataFromMisc.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbDefineFromExcel2.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbDefineFromExcel2.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbDemoGroup.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbDemoGroup.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbDemoGroup_C.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbDemoGroup_C.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbDemoPrimitive.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbDemoPrimitive.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbDetectCsvEncoding.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbDetectCsvEncoding.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbExcelFromJson.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbExcelFromJson.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbExcelFromJsonMultiRow.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbExcelFromJsonMultiRow.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbFullTypes.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbFullTypes.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbItem2.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbItem2.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbMultiIndexList.<LoadAsync>d__5>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbMultiIndexList.<LoadAsync>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbMultiRowRecord.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbMultiRowRecord.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbMultiRowTitle.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbMultiRowTitle.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbMultiUnionIndexList.<LoadAsync>d__3>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbMultiUnionIndexList.<LoadAsync>d__3&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbNotIndexList.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbNotIndexList.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbPath.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbPath.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbSingleton.<LoadAsync>d__5>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbSingleton.<LoadAsync>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbTestBeRef.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbTestBeRef.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbTestBeRef2.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbTestBeRef2.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbTestGlobal.<LoadAsync>d__5>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbTestGlobal.<LoadAsync>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbTestIndex.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbTestIndex.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbTestMap.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbTestMap.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbTestMapper.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbTestMapper.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbTestMultiColumn.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbTestMultiColumn.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbTestNull.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbTestNull.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbTestRef.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbTestRef.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbTestScriptableObject.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbTestScriptableObject.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbTestSet.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbTestSet.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbTestSize.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbTestSize.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbTestString.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbTestString.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,Hotfix.Config.TablesComponent.<LoadAsync>d__162>(System.Runtime.CompilerServices.TaskAwaiter&,Hotfix.Config.TablesComponent.<LoadAsync>d__162&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.ai.TbBehaviorTree.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.ai.TbBehaviorTree.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.ai.TbBlackboard.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.ai.TbBlackboard.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.common.TbGlobalConfig.<LoadAsync>d__5>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.common.TbGlobalConfig.<LoadAsync>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.item.TbItem.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.item.TbItem.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.l10n.TbL10NDemo.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.l10n.TbL10NDemo.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.l10n.TbPatchDemo.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.l10n.TbPatchDemo.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.tag.TbTestTag.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.tag.TbTestTag.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbCompositeJsonTable1.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbCompositeJsonTable1.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbCompositeJsonTable2.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbCompositeJsonTable2.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbCompositeJsonTable3.<LoadAsync>d__5>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbCompositeJsonTable3.<LoadAsync>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbDataFromMisc.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbDataFromMisc.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbDefineFromExcel2.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbDefineFromExcel2.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbDemoGroup.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbDemoGroup.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbDemoGroup_C.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbDemoGroup_C.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbDemoPrimitive.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbDemoPrimitive.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbDetectCsvEncoding.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbDetectCsvEncoding.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbExcelFromJson.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbExcelFromJson.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbExcelFromJsonMultiRow.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbExcelFromJsonMultiRow.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbFullTypes.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbFullTypes.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbItem2.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbItem2.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbMultiIndexList.<LoadAsync>d__5>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbMultiIndexList.<LoadAsync>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbMultiRowRecord.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbMultiRowRecord.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbMultiRowTitle.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbMultiRowTitle.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbMultiUnionIndexList.<LoadAsync>d__3>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbMultiUnionIndexList.<LoadAsync>d__3&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbNotIndexList.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbNotIndexList.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbPath.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbPath.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbSingleton.<LoadAsync>d__5>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbSingleton.<LoadAsync>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbTestBeRef.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbTestBeRef.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbTestBeRef2.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbTestBeRef2.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbTestGlobal.<LoadAsync>d__5>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbTestGlobal.<LoadAsync>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbTestIndex.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbTestIndex.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbTestMap.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbTestMap.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbTestMapper.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbTestMapper.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbTestMultiColumn.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbTestMultiColumn.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbTestNull.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbTestNull.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbTestRef.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbTestRef.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbTestScriptableObject.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbTestScriptableObject.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbTestSet.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbTestSet.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbTestSize.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbTestSize.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.Config.test.TbTestString.<LoadAsync>d__2>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.Config.test.TbTestString.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<Cysharp.Threading.Tasks.UniTask.Awaiter<object>,Hotfix.HotfixLauncher.<ConfigLoader>d__3>(Cysharp.Threading.Tasks.UniTask.Awaiter<object>&,Hotfix.HotfixLauncher.<ConfigLoader>d__3&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.TablesComponent.<LoadAsync>d__162>(Hotfix.Config.TablesComponent.<LoadAsync>d__162&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.ai.TbBehaviorTree.<LoadAsync>d__2>(Hotfix.Config.ai.TbBehaviorTree.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.ai.TbBlackboard.<LoadAsync>d__2>(Hotfix.Config.ai.TbBlackboard.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.common.TbGlobalConfig.<LoadAsync>d__5>(Hotfix.Config.common.TbGlobalConfig.<LoadAsync>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.item.TbItem.<LoadAsync>d__2>(Hotfix.Config.item.TbItem.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.l10n.TbL10NDemo.<LoadAsync>d__2>(Hotfix.Config.l10n.TbL10NDemo.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.l10n.TbPatchDemo.<LoadAsync>d__2>(Hotfix.Config.l10n.TbPatchDemo.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.tag.TbTestTag.<LoadAsync>d__2>(Hotfix.Config.tag.TbTestTag.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbCompositeJsonTable1.<LoadAsync>d__2>(Hotfix.Config.test.TbCompositeJsonTable1.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbCompositeJsonTable2.<LoadAsync>d__2>(Hotfix.Config.test.TbCompositeJsonTable2.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbCompositeJsonTable3.<LoadAsync>d__5>(Hotfix.Config.test.TbCompositeJsonTable3.<LoadAsync>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbDataFromMisc.<LoadAsync>d__2>(Hotfix.Config.test.TbDataFromMisc.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbDefineFromExcel2.<LoadAsync>d__2>(Hotfix.Config.test.TbDefineFromExcel2.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbDemoGroup.<LoadAsync>d__2>(Hotfix.Config.test.TbDemoGroup.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbDemoGroup_C.<LoadAsync>d__2>(Hotfix.Config.test.TbDemoGroup_C.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbDemoPrimitive.<LoadAsync>d__2>(Hotfix.Config.test.TbDemoPrimitive.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbDetectCsvEncoding.<LoadAsync>d__2>(Hotfix.Config.test.TbDetectCsvEncoding.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbExcelFromJson.<LoadAsync>d__2>(Hotfix.Config.test.TbExcelFromJson.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbExcelFromJsonMultiRow.<LoadAsync>d__2>(Hotfix.Config.test.TbExcelFromJsonMultiRow.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbFullTypes.<LoadAsync>d__2>(Hotfix.Config.test.TbFullTypes.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbItem2.<LoadAsync>d__2>(Hotfix.Config.test.TbItem2.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbMultiIndexList.<LoadAsync>d__5>(Hotfix.Config.test.TbMultiIndexList.<LoadAsync>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbMultiRowRecord.<LoadAsync>d__2>(Hotfix.Config.test.TbMultiRowRecord.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbMultiRowTitle.<LoadAsync>d__2>(Hotfix.Config.test.TbMultiRowTitle.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbMultiUnionIndexList.<LoadAsync>d__3>(Hotfix.Config.test.TbMultiUnionIndexList.<LoadAsync>d__3&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbNotIndexList.<LoadAsync>d__2>(Hotfix.Config.test.TbNotIndexList.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbPath.<LoadAsync>d__2>(Hotfix.Config.test.TbPath.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbSingleton.<LoadAsync>d__5>(Hotfix.Config.test.TbSingleton.<LoadAsync>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbTestBeRef.<LoadAsync>d__2>(Hotfix.Config.test.TbTestBeRef.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbTestBeRef2.<LoadAsync>d__2>(Hotfix.Config.test.TbTestBeRef2.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbTestGlobal.<LoadAsync>d__5>(Hotfix.Config.test.TbTestGlobal.<LoadAsync>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbTestIndex.<LoadAsync>d__2>(Hotfix.Config.test.TbTestIndex.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbTestMap.<LoadAsync>d__2>(Hotfix.Config.test.TbTestMap.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbTestMapper.<LoadAsync>d__2>(Hotfix.Config.test.TbTestMapper.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbTestMultiColumn.<LoadAsync>d__2>(Hotfix.Config.test.TbTestMultiColumn.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbTestNull.<LoadAsync>d__2>(Hotfix.Config.test.TbTestNull.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbTestRef.<LoadAsync>d__2>(Hotfix.Config.test.TbTestRef.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbTestScriptableObject.<LoadAsync>d__2>(Hotfix.Config.test.TbTestScriptableObject.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbTestSet.<LoadAsync>d__2>(Hotfix.Config.test.TbTestSet.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbTestSize.<LoadAsync>d__2>(Hotfix.Config.test.TbTestSize.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<Hotfix.Config.test.TbTestString.<LoadAsync>d__2>(Hotfix.Config.test.TbTestString.<LoadAsync>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<Hotfix.HotfixLauncher.<ConfigLoader>d__3>(Hotfix.HotfixLauncher.<ConfigLoader>d__3&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,Hotfix.HotfixLauncher.<LoadConfig>d__2>(System.Runtime.CompilerServices.TaskAwaiter&,Hotfix.HotfixLauncher.<LoadConfig>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.HotfixLauncher.<LoadUI>d__1>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.HotfixLauncher.<LoadUI>d__1&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.UI.UILogin.<Login>d__37>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.UI.UILogin.<Login>d__37&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.UI.UIMain.<OnBagBtnClick>d__37>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.UI.UIMain.<OnBagBtnClick>d__37&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.UI.UIPlayerCreate.<OnCreateButtonClick>d__30>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.UI.UIPlayerCreate.<OnCreateButtonClick>d__30&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.UI.UIPlayerList.<OnLoginButtonClick>d__42>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.UI.UIPlayerList.<OnLoginButtonClick>d__42&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,Hotfix.UI.UIPlayerList.<OnShow>d__41>(System.Runtime.CompilerServices.TaskAwaiter<object>&,Hotfix.UI.UIPlayerList.<OnShow>d__41&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Hotfix.HotfixLauncher.<LoadConfig>d__2>(Hotfix.HotfixLauncher.<LoadConfig>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Hotfix.HotfixLauncher.<LoadUI>d__1>(Hotfix.HotfixLauncher.<LoadUI>d__1&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Hotfix.UI.UILogin.<Login>d__37>(Hotfix.UI.UILogin.<Login>d__37&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Hotfix.UI.UIMain.<OnBagBtnClick>d__37>(Hotfix.UI.UIMain.<OnBagBtnClick>d__37&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Hotfix.UI.UIPlayerCreate.<OnCreateButtonClick>d__30>(Hotfix.UI.UIPlayerCreate.<OnCreateButtonClick>d__30&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Hotfix.UI.UIPlayerCreate.<OnShow>d__29>(Hotfix.UI.UIPlayerCreate.<OnShow>d__29&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Hotfix.UI.UIPlayerList.<OnLoginButtonClick>d__42>(Hotfix.UI.UIPlayerList.<OnLoginButtonClick>d__42&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Hotfix.UI.UIPlayerList.<OnPlayerListItemClick>d__44>(Hotfix.UI.UIPlayerList.<OnPlayerListItemClick>d__44&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<Hotfix.UI.UIPlayerList.<OnShow>d__41>(Hotfix.UI.UIPlayerList.<OnShow>d__41&)
		// object UnityEngine.GameObject.AddComponent<object>()
		// object YooAsset.AssetHandle.GetAssetObject<object>()
		// string string.Join<Hotfix.Config.test.DemoE2>(string,System.Collections.Generic.IEnumerable<Hotfix.Config.test.DemoE2>)
		// string string.Join<UnityEngine.Vector3>(string,System.Collections.Generic.IEnumerable<UnityEngine.Vector3>)
		// string string.Join<UnityEngine.Vector4>(string,System.Collections.Generic.IEnumerable<UnityEngine.Vector4>)
		// string string.Join<int>(string,System.Collections.Generic.IEnumerable<int>)
		// string string.Join<long>(string,System.Collections.Generic.IEnumerable<long>)
		// string string.Join<object>(string,System.Collections.Generic.IEnumerable<object>)
	}
}