using UnityEditor;
using UnityEngine;
using System.IO;

namespace REUN.NiceTransformCopyPaste
{
	[CustomEditor(typeof(Transform))]
	[CanEditMultipleObjects]
	public class TransformCopyPasteEditor : Editor
	{
		private static Vector3 _positionBuffer;
		private static Vector3 _rotationBuffer;
		private static Vector3 _scaleBuffer;
		private static TransformData _transformBuffer;
		private static bool _hasPosition;
		private static bool _hasRotation;
		private static bool _hasScale;
		private static bool _hasTransform;

		private Editor _builtinTransformEditor;
		private System.Type _builtinInspectorType;

		private struct TransformData
		{
			public Vector3 Position;
			public Vector3 Rotation;
			public Vector3 Scale;
		}

		private static GUIContent CopyContent;
		private static GUIContent PasteContent;
		private static Texture2D _copyIconTex;
		private static Texture2D _pasteIconTex;
		private static string _scriptDir;

		private const float MiniButtonSize = 19f;
		private const float MiniButtonIconScale = 0.5f;
		private const float MiniButtonsGap = 0f;
		private const float MiniButtonsRowSpacing = 1f;
		private const float MiniButtonsYOffset = 1.5f;

		private void OnEnable()
		{
			if (string.IsNullOrEmpty(_scriptDir))
			{
				var script = MonoScript.FromScriptableObject(this);
				var scriptPath = AssetDatabase.GetAssetPath(script);
				_scriptDir = Path.GetDirectoryName(scriptPath)?.Replace('\\', '/') + "/";
			}

			if (_copyIconTex == null)
			{
				_copyIconTex = LoadIconFromScriptFolder("copy.png")
							   ?? (Texture2D)EditorGUIUtility.IconContent("Clipboard").image;
			}

			if (_pasteIconTex == null)
			{
				_pasteIconTex = LoadIconFromScriptFolder("paste.png")
								?? (Texture2D)EditorGUIUtility.IconContent("d_Clipboard").image;
			}

			if (CopyContent == null)
				CopyContent = new GUIContent(_copyIconTex, "Copy");
			if (PasteContent == null)
				PasteContent = new GUIContent(_pasteIconTex, "Paste");

			var unityEditorAssembly = typeof(Editor).Assembly;
			_builtinInspectorType = unityEditorAssembly.GetType("UnityEditor.TransformInspector");
			if (_builtinInspectorType != null)
			{
				_builtinTransformEditor = CreateEditor(targets, _builtinInspectorType);
			}
		}

		private void OnDisable()
		{
			if (_builtinTransformEditor != null)
			{
				DestroyImmediate(_builtinTransformEditor);
				_builtinTransformEditor = null;
			}
		}

		private static Texture2D LoadIconFromScriptFolder(string fileName)
		{
			if (string.IsNullOrEmpty(_scriptDir)) return null;
			var path = _scriptDir + fileName;
			return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
		}

		public override void OnInspectorGUI()
		{

			DrawTransformButtons();


			if (_builtinTransformEditor != null)
			{

				DrawBuiltinWithSideMiniButtons();
			}
			else
			{

				DrawVector3Field("Position", t => t.localPosition, (t, v) => t.localPosition = v,
					ref _positionBuffer, ref _hasPosition);
				DrawVector3Field("Rotation", t => t.localEulerAngles, (t, v) => t.localEulerAngles = v,
					ref _rotationBuffer, ref _hasRotation);
				DrawVector3Field("Scale", t => t.localScale, (t, v) => t.localScale = v,
					ref _scaleBuffer, ref _hasScale);
			}
		}

		private void DrawTransformButtons()
		{
			const float buttonHeight = 20f;
			const float icon = 10f;

			var copyFull = new GUIContent(" Copy Transform", _copyIconTex)
			{
				tooltip = "Copy position, rotation, and scale"
			};
			var pasteFull = new GUIContent(" Paste Transform", _pasteIconTex)
			{
				tooltip = "Paste position, rotation, and scale"
			};

			var btnStyle = new GUIStyle(EditorStyles.miniButton)
			{
				alignment = TextAnchor.MiddleCenter,
				imagePosition = ImagePosition.ImageLeft,
				fixedHeight = buttonHeight
			};
			int sidePad = Mathf.Max(btnStyle.padding.left, btnStyle.padding.right);
			btnStyle.padding.left = sidePad;
			btnStyle.padding.right = sidePad;

			EditorGUIUtility.SetIconSize(new Vector2(icon, icon));

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button(copyFull, btnStyle, GUILayout.Height(buttonHeight), GUILayout.ExpandWidth(true)))
			{
				var t = (Transform)target;
				_transformBuffer = new TransformData
				{
					Position = t.localPosition,
					Rotation = t.localEulerAngles,
					Scale = t.localScale
				};
				_hasTransform = true;
			}

			GUILayout.Space(4f);

			EditorGUI.BeginDisabledGroup(!_hasTransform);
			if (GUILayout.Button(pasteFull, btnStyle, GUILayout.Height(buttonHeight), GUILayout.ExpandWidth(true)))
			{
				Undo.RecordObjects(targets, "Paste Transform");
				foreach (Transform t in targets)
				{
					t.localPosition = _transformBuffer.Position;
					t.localEulerAngles = _transformBuffer.Rotation;
					t.localScale = _transformBuffer.Scale;
				}
			}
			EditorGUI.EndDisabledGroup();
			EditorGUILayout.EndHorizontal();

			EditorGUIUtility.SetIconSize(Vector2.zero);

			GUILayout.Space(4f);
		}

		private void DrawVector3Field(string label, System.Func<Transform, Vector3> getter,
			System.Action<Transform, Vector3> setter, ref Vector3 buffer, ref bool hasBuffer)
		{
			float buttonSize = MiniButtonSize;
			float iconPixels = Mathf.Max(1f, buttonSize * Mathf.Clamp01(MiniButtonIconScale));
			const float labelWidth = 55f;
			const float xyzLabelWidth = 13f;

			EditorGUILayout.BeginHorizontal();


			EditorGUIUtility.SetIconSize(new Vector2(iconPixels, iconPixels));
			if (GUILayout.Button(CopyContent, GUILayout.Width(buttonSize), GUILayout.Height(buttonSize)))
			{
				buffer = getter((Transform)target);
				hasBuffer = true;
			}

			GUI.enabled = hasBuffer;
			if (GUILayout.Button(PasteContent, GUILayout.Width(buttonSize), GUILayout.Height(buttonSize)))
			{
				Undo.RecordObjects(targets, $"Paste {label}");
				foreach (Transform t in targets)
				{
					setter(t, buffer);
				}
			}
			GUI.enabled = true;

			EditorGUIUtility.SetIconSize(Vector2.zero);

			GUILayout.Space(2f);
			GUILayout.Label(label, EditorStyles.label, GUILayout.Width(labelWidth));

			var value = getter((Transform)target);
			EditorGUI.BeginChangeCheck();
			float prevLabelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = xyzLabelWidth;
			value = EditorGUILayout.Vector3Field(GUIContent.none, value);
			EditorGUIUtility.labelWidth = prevLabelWidth;
			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObjects(targets, $"Change {label}");
				foreach (Transform t in targets)
				{
					setter(t, value);
				}
			}

			EditorGUILayout.EndHorizontal();
		}


		private void DrawBuiltinWithSideMiniButtons()
		{
			float buttonSize = MiniButtonSize;
			float sideWidth = buttonSize * 2f + MiniButtonsGap + 2f;
			float rowH = Mathf.Max(EditorGUIUtility.singleLineHeight, buttonSize);
			float vSpace = MiniButtonsRowSpacing >= 0 ? MiniButtonsRowSpacing : EditorGUIUtility.standardVerticalSpacing;

			EditorGUILayout.BeginHorizontal();
			{

				EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
				{
					_builtinTransformEditor.OnInspectorGUI();
				}
				EditorGUILayout.EndVertical();


				EditorGUILayout.BeginVertical(GUILayout.Width(sideWidth));
				{
					DrawMiniRow(rowH, () =>
					{
						_positionBuffer = ((Transform)target).localPosition;
						_hasPosition = true;
					}, _hasPosition, () =>
					{
						Undo.RecordObjects(targets, "Paste Position");
						foreach (Transform t in targets) t.localPosition = _positionBuffer;
					});

					GUILayout.Space(vSpace);

					DrawMiniRow(rowH, () =>
					{
						_rotationBuffer = ((Transform)target).localEulerAngles;
						_hasRotation = true;
					}, _hasRotation, () =>
					{
						Undo.RecordObjects(targets, "Paste Rotation");
						foreach (Transform t in targets) t.localEulerAngles = _rotationBuffer;
					});

					GUILayout.Space(vSpace);

					DrawMiniRow(rowH, () =>
					{
						_scaleBuffer = ((Transform)target).localScale;
						_hasScale = true;
					}, _hasScale, () =>
					{
						Undo.RecordObjects(targets, "Paste Scale");
						foreach (Transform t in targets) t.localScale = _scaleBuffer;
					});
				}
				EditorGUILayout.EndVertical();
			}
			EditorGUILayout.EndHorizontal();
		}

		private void DrawMiniRow(float rowHeight, System.Action onCopy, bool canPaste, System.Action onPaste)
		{
			float buttonSize = MiniButtonSize;
			float iconPixels = Mathf.Max(1f, buttonSize * Mathf.Clamp01(MiniButtonIconScale));

			Rect r = GUILayoutUtility.GetRect(0, rowHeight, GUILayout.ExpandWidth(true));
			float y = r.y + (r.height - buttonSize) * 0.5f + MiniButtonsYOffset;
			Rect pasteRect = new Rect(r.xMax - buttonSize, y, buttonSize, buttonSize);
			Rect copyRect = new Rect(pasteRect.x - MiniButtonsGap - buttonSize, y, buttonSize, buttonSize);

			EditorGUIUtility.SetIconSize(new Vector2(iconPixels, iconPixels));
			if (GUI.Button(copyRect, CopyContent))
			{
				onCopy?.Invoke();
				GUIUtility.keyboardControl = 0;
			}
			GUI.enabled = canPaste;
			if (GUI.Button(pasteRect, PasteContent))
			{
				onPaste?.Invoke();
				GUIUtility.keyboardControl = 0;
			}
			GUI.enabled = true;
			EditorGUIUtility.SetIconSize(Vector2.zero);
		}
	}
}