﻿// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.IO;
using ICSharpCode.NRefactory.Editor;
using NUnit.Framework;

namespace ICSharpCode.NRefactory.CSharp.Parser
{
	/// <summary>
	/// Provides utilities to check whether positions and/or tokens in an AST are valid.
	/// </summary>
	public static class ConsistencyChecker
	{
		static void PrintNode (AstNode node)
		{
			Console.WriteLine ("Parent:" + node.GetType ());
			Console.WriteLine ("Children:");
			foreach (var c in node.Children)
				Console.WriteLine (c.GetType () +" at:"+ c.StartLocation +"-"+ c.EndLocation + " Role: "+ c.Role);
			Console.WriteLine ("----");
		}
		
		public static void CheckPositionConsistency (AstNode node, string currentFileName, IDocument currentDocument = null)
		{
			if (currentDocument == null)
				currentDocument = new ReadOnlyDocument(File.ReadAllText(currentFileName));
			string comment = "(" + node.GetType ().Name + " at " + node.StartLocation + " in " + currentFileName + ")";
			var pred = node.StartLocation <= node.EndLocation;
			if (!pred)
				PrintNode (node);
			Assert.IsTrue(pred, "StartLocation must be before EndLocation " + comment);
			var prevNodeEnd = node.StartLocation;
			var prevNode = node;
			for (AstNode child = node.FirstChild; child != null; child = child.NextSibling) {
				bool assertion = child.StartLocation >= prevNodeEnd;
				if (!assertion) {
					PrintNode (prevNode);
					PrintNode (node);
				}
				Assert.IsTrue(assertion, currentFileName + ": Child " + child.GetType () +" (" + child.StartLocation  + ")" +" must start after previous sibling " + prevNode.GetType () + "(" + prevNode.StartLocation + ")");
				CheckPositionConsistency(child, currentFileName, currentDocument);
				prevNodeEnd = child.EndLocation;
				prevNode = child;
			}
			Assert.IsTrue(prevNodeEnd <= node.EndLocation, "Last child must end before parent node ends " + comment);
		}
		
		public static void CheckMissingTokens(AstNode node, string currentFileName, IDocument currentDocument = null)
		{
			if (currentDocument == null)
				currentDocument = new ReadOnlyDocument(File.ReadAllText(currentFileName));
			if (node is CSharpTokenNode) {
				Assert.IsNull(node.FirstChild, "Token nodes should not have children");
			} else {
				var prevNodeEnd = node.StartLocation;
				var prevNode = node;
				for (AstNode child = node.FirstChild; child != null; child = child.NextSibling) {
					CheckWhitespace(prevNode, prevNodeEnd, child, child.StartLocation, currentFileName, currentDocument);
					CheckMissingTokens(child, currentFileName, currentDocument);
					prevNode = child;
					prevNodeEnd = child.EndLocation;
				}
				CheckWhitespace(prevNode, prevNodeEnd, node, node.EndLocation, currentFileName, currentDocument);
			}
		}
		
		static void CheckWhitespace(AstNode startNode, TextLocation whitespaceStart, AstNode endNode, TextLocation whitespaceEnd, string currentFileName, IDocument currentDocument)
		{
			Assert.Greater(whitespaceStart.Line, 0);
			Assert.Greater(whitespaceStart.Column, 0);
			Assert.Greater(whitespaceEnd.Line, 0);
			Assert.Greater(whitespaceEnd.Column, 0);
			if (whitespaceStart == whitespaceEnd || startNode == endNode)
				return;
			int start = currentDocument.GetOffset(whitespaceStart.Line, whitespaceStart.Column);
			int end = currentDocument.GetOffset(whitespaceEnd.Line, whitespaceEnd.Column);
			string text = currentDocument.GetText(start, end - start);
			bool assertion = string.IsNullOrWhiteSpace(text);
			if (!assertion) {
				if (startNode.Parent != endNode.Parent)
					PrintNode (startNode.Parent);
				PrintNode (endNode.Parent);
			}
			Assert.IsTrue(assertion, "Expected whitespace between " + startNode.GetType () +":" + whitespaceStart + " and " + endNode.GetType () + ":" + whitespaceEnd
			              + ", but got '" + text + "' (in " + currentFileName + " parent:" + startNode.Parent.GetType () +")");
		}
	}
}
