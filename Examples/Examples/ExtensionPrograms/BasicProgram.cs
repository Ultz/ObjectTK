﻿using ObjectTK.Data.Shaders;
using ObjectTK.Extensions.Shaders;
using ObjectTK.Extensions.Variables;
using OpenTK.Mathematics;

namespace Examples.Examples.Programs {

	[VertexShaderSource("BasicShader.Vertex")]
	[FragmentShaderSource("BasicShader.Fragment")]
	public class BasicProgram {
		public VertexAttributeInfo<Vector3> InPosition { get; set; }
		public UniformInfo<Matrix4> ModelViewProjectionMatrix { get; set; }
	}
}
