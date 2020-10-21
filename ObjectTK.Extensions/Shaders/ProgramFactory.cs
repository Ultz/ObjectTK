﻿using ObjectTK.Data.Shaders;
using ObjectTK.Data.Variables;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ObjectTK.Extensions.Shaders {
	public class Program<T> : Program where T : ProgramVariableInfoCollection, new() {
		public Program(int Handle, VertexShader VertexShader, FragmentShader FragmentShader, Dictionary<string, UniformInfo> Uniforms, Dictionary<string, VertexAttributeInfo> VertexAttributes) :
			base(Handle, VertexShader, FragmentShader, Uniforms, VertexAttributes) {

		}
	}

	public class ProgramFactory {

		public string BaseDirectory { get; set; } = "./";
		public string ShaderExtension { get; set; } = "glsl";

		public Program<T> CreateProgram<T>() where T : ProgramVariableInfoCollection, new() {

			List<ShaderSourceAttribute> Attributes = typeof(T).GetCustomAttributes<ShaderSourceAttribute>(true).ToList();
			List<Shader> Shaders = new List<Shader>();

			int ProgramHandle = GL.CreateProgram();

			foreach (ShaderSourceAttribute Attribute in Attributes) {
				string Source = GetEffectSource(Attribute.EffectKey);

				int ShaderHandle = GL.CreateShader(ShaderType.VertexShader);
				GL.ShaderSource(ShaderHandle, Source);
				GL.CompileShader(ShaderHandle);
				GL.AttachShader(ProgramHandle, ShaderHandle);

				switch (Attribute.Type) {
					case ShaderType.FragmentShader:
						Shaders.Add(new FragmentShader(ShaderHandle, Source));
						break;
					case ShaderType.VertexShader:
						Shaders.Add(new VertexShader(ShaderHandle, Source));
						break;
					default:
						break;
				}
			}

			GL.LinkProgram(ProgramHandle);

			foreach(Shader Shader in Shaders) {
				GL.DetachShader(ProgramHandle, Shader.Handle);
				GL.DeleteShader(Shader.Handle);
			}

			int UniformLocation = GL.GetUniformLocation(ProgramHandle, "ModelViewProjectionMatrix");
			GL.GetActiveUniform(ProgramHandle, UniformLocation, out int UniformSize, out ActiveUniformType UniformType);
			UniformInfo UI_InPosition = new UniformInfo(ProgramHandle, "ModelViewProjectionMatrix", UniformLocation, UniformSize, UniformType, UniformLocation > -1);

			Program<T> ShaderProgram = new Program<T>(ProgramHandle, null, null, new UniformInfo[] { UI_InPosition }, new VertexAttributeInfo[] { });
			InitializeProgramVariables(ShaderProgram);

			return ShaderProgram;
		}

		private string GetEffectSource(string EffectKey, Dictionary<string, string> Effects = null) {
			
			Effects ??= new Dictionary<string, string>();

			string EffectFile = Path.ChangeExtension(EffectKey, null);
			string FilePath = Path.Join(BaseDirectory, Path.ChangeExtension(EffectKey, "glsl"));
			string FullSource = File.ReadAllText(FilePath);

			ExtractEffects(EffectFile, FullSource, Effects);

			Effects.TryGetValue(EffectKey, out string EffectSource);
			return EffectSource ?? throw new Exception($"Error loading effect with key: {EffectKey}");
		}
		private void ExtractEffects(string EffectFile, string FullSource, Dictionary<string, string> Effects) {

			const string EffectToken = "--";
			const string EffectFileSeparator = ".";
			const string IncludeToken = "#include";

			string[] Lines = FullSource.Split('\n');

			StringBuilder Effect = new StringBuilder();
			string EffectKey = null;

			foreach(string Line in Lines) {
				if (Line.StartsWith(EffectToken) && Line.Length > EffectToken.Length) {
					if(EffectKey != null) {
						Effects.Add(EffectKey, Effect.ToString());
						Effect.Clear();
						EffectKey = null;
					}
					EffectKey = $"{EffectFile}{EffectFileSeparator}{Line.Substring(EffectToken.Length).Trim()}";
					continue;
				}

				if(EffectKey != null) {
					if(Line.StartsWith(IncludeToken) && Line.Length > IncludeToken.Length) {
						string IncludedEffect = Line.Substring(IncludeToken.Length).Trim();
						if(!Effects.TryGetValue(IncludedEffect, out string EffectSource)) {
							EffectSource = GetEffectSource(IncludedEffect, Effects);
						}
						Effect.AppendLine(EffectSource);
					} else {
						Effect.AppendLine(Line);
					}
				}
			}
			if (EffectKey != null) {
				Effects.Add(EffectKey, Effect.ToString());
				Effect.Clear();
			}
		}

		private void InitializeProgramVariables<T>(Program<T> Program) where T : ProgramVariableInfoCollection, new() {

		}
	}

}
