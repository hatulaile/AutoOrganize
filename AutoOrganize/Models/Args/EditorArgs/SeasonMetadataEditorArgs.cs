using System.Collections.Generic;
using AutoOrganize.Models.MetadataNodes.Metadata;

namespace AutoOrganize.Models.Args.EditorArgs;

public readonly record struct SeasonMetadataEditorArgs(IReadOnlyList<SeasonMetadataTreeNode> Nodes);