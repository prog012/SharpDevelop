﻿/*
 * Created by SharpDevelop.
 * User: Peter Forstmeier
 * Date: 06.06.2013
 * Time: 20:27
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

using ICSharpCode.Reporting.DataManager.Listhandling;
using ICSharpCode.Reporting.Exporter.Visitors;
using ICSharpCode.Reporting.Interfaces;
using ICSharpCode.Reporting.Interfaces.Export;
using ICSharpCode.Reporting.Items;
using ICSharpCode.Reporting.PageBuilder.Converter;
using ICSharpCode.Reporting.PageBuilder.ExportColumns;

namespace ICSharpCode.Reporting.PageBuilder
{
	/// <summary>
	/// Description of DataPageBuilder.
	/// </summary>
	public class DataPageBuilder:BasePageBuilder
	{
		
		public DataPageBuilder(IReportModel reportModel,IEnumerable list):base(reportModel)
		{
			List = list;
		}
		
		
		public override void BuildExportList()
		{
			CreateDataSource();
			base.BuildExportList();
			BuildDetail();
			BuildReportFooter();
			AddPage(CurrentPage);
			UpdatePageInfo();
			RunExpressions(ReportModel.ReportSettings,DataSource);
			var formatVisitor = new FormatVisitor();
			formatVisitor.Run(Pages);
			var dv = new DebugVisitor();
			dv.Run(Pages);
		}
		
		
		void BuildDetail()
		{
			CurrentSection = ReportModel.DetailSection;
			if(DataSourceContainsData()) {
				CurrentLocation = DetailStart;
				if (IsGrouped()) {
					BuildGroupedDetails();
				} else {
					BuildSortedDetails();
				}
			}
		}

		
		void BuildGroupedDetails () {
			
			var exportRows = new List<IExportContainer>();
			var converter = new ContainerConverter(base.Graphics, CurrentLocation);
			var position = DetailStart;
			
			foreach (IGrouping<object, object> grouping in DataSource.GroupedList) {
				
				var groupHeader = (BaseRowItem)CurrentSection.Items.Where(p => p.GetType() == typeof(GroupHeader)).FirstOrDefault();
				DataSource.Fill(groupHeader.Items,grouping.FirstOrDefault());
				
				var sectionContainer = CreateContainerForSection(CurrentPage, position);
				
				var headerRow = converter.ConvertToExportContainer(groupHeader);
				headerRow.Location = new Point(headerRow.Location.X,groupHeader.Location.Y);
				
				var headerItems = converter.CreateConvertedList(groupHeader.Items);
				headerRow.ExportedItems.AddRange(headerItems);
				
				sectionContainer.ExportedItems.Add(headerRow);
				exportRows.Add(sectionContainer);
				
				position = new Point(CurrentSection.Location.X, position.Y + sectionContainer.DesiredSize.Height + 1);
				
//				position = new Point(position.X,headerRow.DisplayRectangle.Bottom + 2);
				
				foreach (var current in grouping) {
				
					DataSource.Fill(CurrentSection.Items,current);
					/*	
					var ll = CurrentSection.Items.Where(p => p.GetType() == typeof(BaseDataItem));
					var convertedItems = converter.CreateConvertedList(ll.ToList());
					foreach (var element in convertedItems) {
						element.Location = new Point(element.Location.X,position.Y);
					}
//					converter.SetParent(row, convertedItems);
*/
/*
					if (PageFull(row)) {
						InsertExportRows(exportRows);
						exportRows.Clear();
						PerformPageBreak();
						position = DetailStart;
						row.Location = position;
					}
*/
/*					
					sectionContainer.ExportedItems.AddRange(convertedItems);
					MeasureAndArrangeContainer(sectionContainer);
//					row.ExportedItems.AddRange(convertedItems);
					exportRows.Add(sectionContainer);
					position = new Point(CurrentSection.Location.X, position.Y + convertedItems[0].DesiredSize.Height + 1);
//					position = new Point(CurrentSection.Location.X, position.Y + sectionContainer.DesiredSize.Height + 1);
*/
				}
			
			}
			InsertExportRows(exportRows);
		}
		
		void BuildSortedDetails(){
			
			var exportRows = new List<IExportContainer>();
			var converter = new ContainerConverter(base.Graphics, CurrentLocation);
			var position = DetailStart;
			
			foreach (var element in DataSource.SortedList) {
				
				var row = CreateContainerForSection(CurrentPage, position);
				DataSource.Fill(CurrentSection.Items,element);

				var convertedItems = converter.CreateConvertedList(ReportModel.DetailSection.Items);

				converter.SetParent(row, convertedItems);

				if (PageFull(row)) {
					InsertExportRows(exportRows);
					exportRows.Clear();
					PerformPageBreak();
					position = DetailStart;
					row.Location = position;
				}

				MeasureAndArrangeContainer(row);

				row.ExportedItems.AddRange(convertedItems);
				exportRows.Add(row);
				position = new Point(CurrentSection.Location.X, position.Y + row.DesiredSize.Height + 1);
			}

			InsertExportRows(exportRows);
		}
		
		/*
		void old_BuildSortedDetails(){
		
			var exportRows = new List<IExportContainer>();
			var converter = new ContainerConverter(base.Graphics, CurrentLocation);
			var position = DetailStart;
			
			do {
				var row = CreateContainerForSection(CurrentPage, position);
				DataSource.Fill(CurrentSection.Items);

				var convertedItems = converter.CreateConvertedList(ReportModel.DetailSection.Items);

				converter.SetParent(row, convertedItems);

				if (PageFull(row)) {
					InsertExportRows(exportRows);
					exportRows.Clear();
					PerformPageBreak();
					position = DetailStart;
					row.Location = position;
				}

				MeasureAndArrangeContainer(row);

				row.ExportedItems.AddRange(convertedItems);
				exportRows.Add(row);
				position = new Point(CurrentSection.Location.X, position.Y + row.DesiredSize.Height + 1);

			} while (DataSource.MoveNext());
			InsertExportRows(exportRows);
		}
*/
		
		void PerformPageBreak(){
			CurrentPage.PageInfo.PageNumber = Pages.Count + 1;
			Pages.Add(CurrentPage);
			CurrentPage = CreateNewPage();
			WriteStandardSections();
			CurrentLocation = DetailStart;
			ResetPosition();
		}

		
		bool IsGrouped(){
			return DataSource.OrderGroup == OrderGroup.Grouped;
		}

		
		void CreateDataSource(){
			DataSource = new CollectionDataSource(List, ReportModel.ReportSettings);
			if (DataSourceContainsData()) {
				DataSource.Bind();
			}
		}
		
		
		bool DataSourceContainsData () {
			if (DataSource.Count > 0) {
				return true;
			}
			return false;
		}
		

		
		Point ResetPosition () {
			return new Point(DetailStart.X,1);
		}
		
		
		void MeasureAndArrangeContainer(IExportContainer container){
			container.DesiredSize = MeasureElement(container);
			ArrangeContainer(container);
		}

		
		ExportContainer CreateContainerForSection(ExportPage parent,Point location ){
			var detail = (ExportContainer)CurrentSection.CreateExportColumn();
			detail.Location = location;
			detail.Parent = parent;
			return detail;
		}
		
		
		void InsertExportRows(List<IExportContainer> list){
			if (Pages.Count == 0) {
				CurrentPage.ExportedItems.InsertRange(2, list);
			} else {
				CurrentPage.ExportedItems.InsertRange(1, list);
			}
		}
		
		
		internal CollectionDataSource DataSource {get; private set;}
		
		internal IEnumerable List {get; private set;}
		
		protected IReportContainer CurrentSection { get; private set; }
		
	}
}