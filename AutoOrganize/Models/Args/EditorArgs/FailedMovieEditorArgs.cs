using System.Collections.Generic;
using AutoOrganize.Models.MetadataNodes.Abstractions;

namespace AutoOrganize.Models.Args.EditorArgs;

public readonly record struct FailedMovieEditorArgs(IReadOnlyList<IFailedNode> Nodes);