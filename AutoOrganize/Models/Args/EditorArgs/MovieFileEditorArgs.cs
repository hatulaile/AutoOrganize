using System.Collections.Generic;
using AutoOrganize.Models.MetadataNodes.FileSystem;

namespace AutoOrganize.Models.Args.EditorArgs;

public readonly record struct MovieFileEditorArgs(IReadOnlyList<SourceFileNode> Nodes);