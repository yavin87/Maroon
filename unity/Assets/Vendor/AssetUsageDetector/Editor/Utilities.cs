﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
#if UNITY_2018_1_OR_NEWER
using Unity.Collections;
#endif
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
#if UNITY_2018_3_OR_NEWER
using PrefabStage = UnityEditor.Experimental.SceneManagement.PrefabStage;
using PrefabStageUtility = UnityEditor.Experimental.SceneManagement.PrefabStageUtility;
#endif

namespace AssetUsageDetectorNamespace
{
	public static class Utilities
	{
		// A set of commonly used Unity types
		private static readonly HashSet<Type> primitiveUnityTypes = new HashSet<Type>()
		{
			typeof( string ), typeof( Vector4 ), typeof( Vector3 ), typeof( Vector2 ), typeof( Rect ),
			typeof( Quaternion ), typeof( Color ), typeof( Color32 ), typeof( LayerMask ), typeof( Bounds ),
			typeof( Matrix4x4 ), typeof( AnimationCurve ), typeof( Gradient ), typeof( RectOffset ),
			typeof( bool[] ), typeof( byte[] ), typeof( sbyte[] ), typeof( char[] ), typeof( decimal[] ),
			typeof( double[] ), typeof( float[] ), typeof( int[] ), typeof( uint[] ), typeof( long[] ),
			typeof( ulong[] ), typeof( short[] ), typeof( ushort[] ), typeof( string[] ),
			typeof( Vector4[] ), typeof( Vector3[] ), typeof( Vector2[] ), typeof( Rect[] ),
			typeof( Quaternion[] ), typeof( Color[] ), typeof( Color32[] ), typeof( LayerMask[] ), typeof( Bounds[] ),
			typeof( Matrix4x4[] ), typeof( AnimationCurve[] ), typeof( Gradient[] ), typeof( RectOffset[] ),
			typeof( List<bool> ), typeof( List<byte> ), typeof( List<sbyte> ), typeof( List<char> ), typeof( List<decimal> ),
			typeof( List<double> ), typeof( List<float> ), typeof( List<int> ), typeof( List<uint> ), typeof( List<long> ),
			typeof( List<ulong> ), typeof( List<short> ), typeof( List<ushort> ), typeof( List<string> ),
			typeof( List<Vector4> ), typeof( List<Vector3> ), typeof( List<Vector2> ), typeof( List<Rect> ),
			typeof( List<Quaternion> ), typeof( List<Color> ), typeof( List<Color32> ), typeof( List<LayerMask> ), typeof( List<Bounds> ),
			typeof( List<Matrix4x4> ), typeof( List<AnimationCurve> ), typeof( List<Gradient> ), typeof( List<RectOffset> ),
#if UNITY_2017_2_OR_NEWER
			typeof( Vector3Int ), typeof( Vector2Int ), typeof( RectInt ), typeof( BoundsInt ),
			typeof( Vector3Int[] ), typeof( Vector2Int[] ), typeof( RectInt[] ), typeof( BoundsInt[] ),
			typeof( List<Vector3Int> ), typeof( List<Vector2Int> ), typeof( List<RectInt> ), typeof( List<BoundsInt> )
#endif
		};

		private static readonly string reflectionNamespace = typeof( Assembly ).Namespace;
#if UNITY_2018_1_OR_NEWER
		private static readonly string nativeCollectionsNamespace = typeof( NativeArray<int> ).Namespace;
#endif

		private static readonly HashSet<string> folderContentsSet = new HashSet<string>();

		private static readonly StringBuilder stringBuilder = new StringBuilder( 10 );

#if UNITY_2018_3_OR_NEWER
		private static int previousPingedPrefabInstanceId;
		private static double previousPingedPrefabPingTime;
#endif

		public static readonly GUILayoutOption GL_EXPAND_WIDTH = GUILayout.ExpandWidth( true );
		public static readonly GUILayoutOption GL_EXPAND_HEIGHT = GUILayout.ExpandHeight( true );
		public static readonly GUILayoutOption GL_WIDTH_25 = GUILayout.Width( 25 );
		public static readonly GUILayoutOption GL_WIDTH_100 = GUILayout.Width( 100 );
		public static readonly GUILayoutOption GL_WIDTH_250 = GUILayout.Width( 250 );
		public static readonly GUILayoutOption GL_HEIGHT_0 = GUILayout.Height( 0 );
		public static readonly GUILayoutOption GL_HEIGHT_2 = GUILayout.Height( 2 );
		public static readonly GUILayoutOption GL_HEIGHT_30 = GUILayout.Height( 30 );
		public static readonly GUILayoutOption GL_HEIGHT_35 = GUILayout.Height( 35 );
		public static readonly GUILayoutOption GL_HEIGHT_40 = GUILayout.Height( 40 );

		private static GUIStyle m_boxGUIStyle; // GUIStyle used to draw the results of the search
		public static GUIStyle BoxGUIStyle
		{
			get
			{
				if( m_boxGUIStyle == null )
				{
					m_boxGUIStyle = new GUIStyle( EditorStyles.helpBox )
					{
						alignment = TextAnchor.MiddleCenter,
						font = EditorStyles.label.font
					};

					Color textColor = GUI.skin.button.normal.textColor;
					m_boxGUIStyle.normal.textColor = textColor;
					m_boxGUIStyle.hover.textColor = textColor;
					m_boxGUIStyle.focused.textColor = textColor;
					m_boxGUIStyle.active.textColor = textColor;

#if !UNITY_2019_1_OR_NEWER || UNITY_2019_3_OR_NEWER
					// On 2019.1 and 2019.2 versions, GUI.skin.button.fontSize returns 0 on some devices
					// https://forum.unity.com/threads/asset-usage-detector-find-references-to-an-asset-object-open-source.408134/page-3#post-7285954
					m_boxGUIStyle.fontSize = ( m_boxGUIStyle.fontSize + GUI.skin.button.fontSize ) / 2;
#endif
				}

				return m_boxGUIStyle;
			}
		}

		private static GUIStyle m_tooltipGUIStyle; // GUIStyle used to draw the tooltip
		public static GUIStyle TooltipGUIStyle
		{
			get
			{
				GUIStyleState normalState;

				if( m_tooltipGUIStyle != null )
					normalState = m_tooltipGUIStyle.normal;
				else
				{
					m_tooltipGUIStyle = new GUIStyle( EditorStyles.helpBox )
					{
						alignment = TextAnchor.MiddleCenter,
						font = EditorStyles.label.font
					};

					normalState = m_tooltipGUIStyle.normal;

					normalState.background = null;
					normalState.scaledBackgrounds = new Texture2D[0];
					normalState.textColor = Color.black;
				}

				if( normalState.background == null || normalState.background.Equals( null ) )
				{
					Texture2D backgroundTexture = new Texture2D( 1, 1 ) { hideFlags = HideFlags.HideAndDontSave };
					backgroundTexture.SetPixel( 0, 0, new Color( 0.88f, 0.88f, 0.88f, 0.85f ) );
					backgroundTexture.Apply();

					normalState.background = backgroundTexture;
				}

				return m_tooltipGUIStyle;
			}
		}

		// Get a unique-ish string hash code for an object
		public static string Hash( this object obj )
		{
			if( obj is Object )
				return obj.GetHashCode().ToString();

			return obj.GetHashCode() + obj.GetType().Name;
		}

		// Check if object is an asset or a Scene object
		public static bool IsAsset( this object obj )
		{
			return obj is Object && AssetDatabase.Contains( (Object) obj );
		}

		// Check if object is a folder asset
		public static bool IsFolder( this Object obj )
		{
			return obj is DefaultAsset && AssetDatabase.IsValidFolder( AssetDatabase.GetAssetPath( obj ) );
		}

		// Returns an enumerator to iterate through all asset paths in the folder
		public static IEnumerable<string> EnumerateFolderContents( Object folderAsset )
		{
			string[] folderContents = AssetDatabase.FindAssets( "", new string[] { AssetDatabase.GetAssetPath( folderAsset ) } );
			if( folderContents == null )
				return new EmptyEnumerator<string>();

			folderContentsSet.Clear();
			for( int i = 0; i < folderContents.Length; i++ )
			{
				string filePath = AssetDatabase.GUIDToAssetPath( folderContents[i] );
				if( !string.IsNullOrEmpty( filePath ) && !AssetDatabase.IsValidFolder( filePath ) )
					folderContentsSet.Add( filePath );
			}

			return folderContentsSet;
		}

		// Select an object in the editor
		public static void SelectInEditor( this Object obj )
		{
			if( obj == null )
				return;

			Event e = Event.current;

			// If CTRL or Shift keys are pressed, do a multi-select;
			// otherwise select only the clicked object and ping it in editor
			if( !e.control && !e.shift )
				Selection.activeObject = obj.PingInEditor();
			else
			{
				Component objAsComp = obj as Component;
				GameObject objAsGO = obj as GameObject;
				int selectionIndex = -1;

				Object[] selection = Selection.objects;
				for( int i = 0; i < selection.Length; i++ )
				{
					Object selected = selection[i];

					// Don't allow including both a gameobject and one of its components in the selection
					if( selected == obj || ( objAsComp != null && selected == objAsComp.gameObject ) ||
						( objAsGO != null && selected is Component && ( (Component) selected ).gameObject == objAsGO ) )
					{
						selectionIndex = i;
						break;
					}
				}

				Object[] newSelection;
				if( selectionIndex == -1 )
				{
					// Include object in selection
					newSelection = new Object[selection.Length + 1];
					selection.CopyTo( newSelection, 0 );
					newSelection[selection.Length] = obj;
				}
				else
				{
					// Remove object from selection
					newSelection = new Object[selection.Length - 1];
					int j = 0;
					for( int i = 0; i < selectionIndex; i++, j++ )
						newSelection[j] = selection[i];
					for( int i = selectionIndex + 1; i < selection.Length; i++, j++ )
						newSelection[j] = selection[i];
				}

				Selection.objects = newSelection;
			}
		}

		// Ping an object in either Project view or Hierarchy view
		public static Object PingInEditor( this Object obj )
		{
			if( obj is Component )
				obj = ( (Component) obj ).gameObject;

			Object selection = obj;

			// Pinging a prefab only works if the pinged object is the root of the prefab
			// or a direct child of it. Pinging any grandchildren of the prefab
			// does not work; in which case, traverse the parent hierarchy until
			// a pingable parent is reached
			if( obj.IsAsset() && obj is GameObject )
			{
#if UNITY_2018_3_OR_NEWER
				Transform objTR = ( (GameObject) obj ).transform.root;

				PrefabAssetType prefabAssetType = PrefabUtility.GetPrefabAssetType( objTR.gameObject );
				if( prefabAssetType == PrefabAssetType.Regular || prefabAssetType == PrefabAssetType.Variant )
				{
					string assetPath = AssetDatabase.GetAssetPath( objTR.gameObject );
					PrefabStage openPrefabStage = PrefabStageUtility.GetCurrentPrefabStage();

					// Try to open the prefab stage of pinged prefabs if they are double clicked
					if( previousPingedPrefabInstanceId == objTR.GetInstanceID() && EditorApplication.timeSinceStartup - previousPingedPrefabPingTime <= 0.3f &&
#if UNITY_2020_1_OR_NEWER
						( openPrefabStage == null || !openPrefabStage.stageHandle.IsValid() || assetPath != openPrefabStage.assetPath ) )
#else
						( openPrefabStage == null || !openPrefabStage.stageHandle.IsValid() || assetPath != openPrefabStage.prefabAssetPath ) )
#endif
					{
						AssetDatabase.OpenAsset( objTR.gameObject );
						openPrefabStage = PrefabStageUtility.GetCurrentPrefabStage();
					}

					previousPingedPrefabInstanceId = objTR.GetInstanceID();
					previousPingedPrefabPingTime = EditorApplication.timeSinceStartup;

#if UNITY_2020_1_OR_NEWER
					if( openPrefabStage != null && openPrefabStage.stageHandle.IsValid() && assetPath == openPrefabStage.assetPath )
#else
					if( openPrefabStage != null && openPrefabStage.stageHandle.IsValid() && assetPath == openPrefabStage.prefabAssetPath )
#endif
					{
						GameObject prefabStageGO = FollowSymmetricHierarchy( (GameObject) obj, ( (GameObject) obj ).transform.root.gameObject, openPrefabStage.prefabContentsRoot );
						if( prefabStageGO != null )
						{
							objTR = prefabStageGO.transform;
							selection = objTR.gameObject;
						}
					}
#if UNITY_2019_1_OR_NEWER
					else if( obj != objTR.gameObject )
					{
						Debug.Log( "Open " + assetPath + " in prefab mode to select and edit " + obj.name );
						selection = objTR.gameObject;
					}
#else
					else
						Debug.Log( "Open " + assetPath + " in prefab mode to select and edit " + obj.name );
#endif
				}
#else
				Transform objTR = ( (GameObject) obj ).transform;
				while( objTR.parent != null && objTR.parent.parent != null )
					objTR = objTR.parent;
#endif

				obj = objTR.gameObject;
			}

			EditorGUIUtility.PingObject( obj );
			return selection;
		}

		// We are passing "go"s root Transform to thisRoot parameter. If we use go.transform.root instead, when we are in prefab mode on
		// newer Unity versions, it points to the preview scene at the root of the prefab stage instead of pointing to the actual root of "go"
		public static GameObject FollowSymmetricHierarchy( this GameObject go, GameObject thisRoot, GameObject symmetricRoot )
		{
			Transform target = go.transform;
			Transform root1 = thisRoot.transform;
			Transform root2 = symmetricRoot.transform;
			while( root1 != target )
			{
				Transform temp = target;
				while( temp.parent != root1 )
					temp = temp.parent;

				Transform newRoot2;
				int siblingIndex = temp.GetSiblingIndex();
				if( siblingIndex < root2.childCount )
				{
					newRoot2 = root2.GetChild( siblingIndex );
					if( newRoot2.name != temp.name )
						newRoot2 = root2.Find( temp.name );
				}
				else
					newRoot2 = root2.Find( temp.name );

				if( newRoot2 == null )
					return null;

				root2 = newRoot2;
				root1 = temp;
			}

			return root2.gameObject;
		}

		// Check if the field is serializable
		public static bool IsSerializable( this FieldInfo fieldInfo )
		{
			// See Serialization Rules: https://docs.unity3d.com/Manual/script-Serialization.html
			if( fieldInfo.IsInitOnly )
				return false;

#if UNITY_2019_3_OR_NEWER
			// SerializeReference makes even System.Object fields serializable
			if( Attribute.IsDefined( fieldInfo, typeof( SerializeReference ) ) )
				return true;
#endif

			if( ( !fieldInfo.IsPublic || fieldInfo.IsNotSerialized ) && !Attribute.IsDefined( fieldInfo, typeof( SerializeField ) ) )
				return false;

			return IsTypeSerializable( fieldInfo.FieldType );
		}

		// Check if the property is serializable
		public static bool IsSerializable( this PropertyInfo propertyInfo )
		{
			return IsTypeSerializable( propertyInfo.PropertyType );
		}

		// Check if type is serializable
		private static bool IsTypeSerializable( Type type )
		{
			// see Serialization Rules: https://docs.unity3d.com/Manual/script-Serialization.html
			if( typeof( Object ).IsAssignableFrom( type ) )
				return true;

			if( type.IsArray )
			{
				if( type.GetArrayRank() != 1 )
					return false;

				type = type.GetElementType();

				if( typeof( Object ).IsAssignableFrom( type ) )
					return true;
			}
			else if( type.IsGenericType )
			{
				// Generic types are allowed on 2020.1 and later
#if UNITY_2020_1_OR_NEWER
				if( type.GetGenericTypeDefinition() == typeof( List<> ) )
				{
					type = type.GetGenericArguments()[0];

					if( typeof( Object ).IsAssignableFrom( type ) )
						return true;
				}
#else
				if( type.GetGenericTypeDefinition() != typeof( List<> ) )
					return false;

				type = type.GetGenericArguments()[0];

				if( typeof( Object ).IsAssignableFrom( type ) )
					return true;
#endif
			}

#if !UNITY_2020_1_OR_NEWER
			if( type.IsGenericType )
				return false;
#endif

			return Attribute.IsDefined( type, typeof( SerializableAttribute ), false );
		}

		// Check if instances of this type should be searched for references
		public static bool IsIgnoredUnityType( this Type type )
		{
			if( type.IsPrimitive || primitiveUnityTypes.Contains( type ) || type.IsEnum )
				return true;

#if UNITY_2018_1_OR_NEWER
			// Searching NativeArrays for reference can throw InvalidOperationException if the collection is disposed
			if( type.Namespace == nativeCollectionsNamespace )
				return true;
#endif

			// Searching assembly variables for reference throws InvalidCastException on .NET 4.0 runtime
			if( typeof( Type ).IsAssignableFrom( type ) || type.Namespace == reflectionNamespace )
				return true;

			// Searching pointers or ref variables for reference throws ArgumentException
			if( type.IsPointer || type.IsByRef )
				return true;

			return false;
		}

		// Get <get> function for a field
		public static VariableGetVal CreateGetter( this FieldInfo fieldInfo, Type type )
		{
			// Commented the IL generator code below because it might actually be slower than simply using reflection
			// Credit: https://www.codeproject.com/Articles/14560/Fast-Dynamic-Property-Field-Accessors
			//DynamicMethod dm = new DynamicMethod( "Get" + fieldInfo.Name, fieldInfo.FieldType, new Type[] { typeof( object ) }, type );
			//ILGenerator il = dm.GetILGenerator();
			//// Load the instance of the object (argument 0) onto the stack
			//il.Emit( OpCodes.Ldarg_0 );
			//// Load the value of the object's field (fi) onto the stack
			//il.Emit( OpCodes.Ldfld, fieldInfo );
			//// return the value on the top of the stack
			//il.Emit( OpCodes.Ret );

			//return (VariableGetVal) dm.CreateDelegate( typeof( VariableGetVal ) );

			return fieldInfo.GetValue;
		}

		// Get <get> function for a property
		public static VariableGetVal CreateGetter( this PropertyInfo propertyInfo )
		{
			// Can't use PropertyWrapper (which uses CreateDelegate) for property getters of structs
			if( propertyInfo.DeclaringType.IsValueType )
			{
				return !propertyInfo.CanRead ? (VariableGetVal) null : ( obj ) =>
				{
					try
					{
						return propertyInfo.GetValue( obj, null );
					}
					catch
					{
						// Property getters may return various kinds of exceptions if their backing fields are not initialized (yet)
						return null;
					}
				};
			}

			Type GenType = typeof( PropertyWrapper<,> ).MakeGenericType( propertyInfo.DeclaringType, propertyInfo.PropertyType );
			return ( (IPropertyAccessor) Activator.CreateInstance( GenType, propertyInfo.GetGetMethod( true ) ) ).GetValue;
		}

		// Check if all open scenes are saved (not dirty)
		public static bool AreScenesSaved()
		{
			for( int i = 0; i < EditorSceneManager.loadedSceneCount; i++ )
			{
				Scene scene = EditorSceneManager.GetSceneAt( i );
				if( scene.isDirty || string.IsNullOrEmpty( scene.path ) )
					return false;
			}

			return true;
		}

		// Returns file extension in lowercase (period not included)
		public static string GetFileExtension( string path )
		{
			int extensionIndex = path.LastIndexOf( '.' );
			if( extensionIndex < 0 || extensionIndex >= path.Length - 1 )
				return "";

			stringBuilder.Length = 0;
			for( extensionIndex++; extensionIndex < path.Length; extensionIndex++ )
			{
				char ch = path[extensionIndex];
				if( ch >= 65 && ch <= 90 ) // A-Z
					ch += (char) 32; // Converted to a-z

				stringBuilder.Append( ch );
			}

			return stringBuilder.ToString();
		}

		// Draw horizontal line inside OnGUI
		public static void DrawSeparatorLine()
		{
			GUILayout.Space( 4 );
			GUILayout.Box( "", GL_HEIGHT_2, GL_EXPAND_WIDTH );
			GUILayout.Space( 4 );
		}

		// Check if all the objects inside the list are null
		public static bool IsEmpty( this List<ObjectToSearch> objectsToSearch )
		{
			if( objectsToSearch == null )
				return true;

			for( int i = 0; i < objectsToSearch.Count; i++ )
			{
				if( objectsToSearch[i].obj != null && !objectsToSearch[i].obj.Equals( null ) )
					return false;
			}

			return true;
		}

		// Check if all the objects inside the list are null
		public static bool IsEmpty( this List<Object> objects )
		{
			if( objects == null )
				return true;

			for( int i = 0; i < objects.Count; i++ )
			{
				if( objects[i] != null && !objects[i].Equals( null ) )
					return false;
			}

			return true;
		}

		// Check if all the objects that are enumerated are null
		public static bool IsEmpty( this IEnumerable<Object> objects )
		{
			if( objects == null )
				return true;

			using( IEnumerator<Object> enumerator = objects.GetEnumerator() )
			{
				while( enumerator.MoveNext() )
				{
					if( enumerator.Current != null && !enumerator.Current.Equals( null ) )
						return false;
				}
			}

			return true;
		}

		// Returns true is str starts with prefix
		public static bool StartsWithFast( this string str, string prefix )
		{
			int aLen = str.Length;
			int bLen = prefix.Length;
			int ap = 0; int bp = 0;
			while( ap < aLen && bp < bLen && str[ap] == prefix[bp] )
			{
				ap++;
				bp++;
			}

			return bp == bLen;
		}

		// Returns true is str ends with postfix
		public static bool EndsWithFast( this string str, string postfix )
		{
			int ap = str.Length - 1;
			int bp = postfix.Length - 1;
			while( ap >= 0 && bp >= 0 && str[ap] == postfix[bp] )
			{
				ap--;
				bp--;
			}

			return bp < 0;
		}

		public static bool ContainsFast<T>( this List<T> list, T element )
		{
			if( !( element is ValueType ) )
			{
				for( int i = list.Count - 1; i >= 0; i-- )
				{
					if( ReferenceEquals( list[i], element ) )
						return true;
				}
			}
			else
			{
				for( int i = list.Count - 1; i >= 0; i-- )
				{
					if( element.Equals( list[i] ) )
						return true;
				}
			}

			return false;
		}

		public static void RemoveAtFast<T>( this List<T> list, int index )
		{
			int lastElementIndex = list.Count - 1;

			list[index] = list[lastElementIndex];
			list.RemoveAt( lastElementIndex );
		}
	}
}