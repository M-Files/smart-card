using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MFiles.PdfTools.UIExt
{
	public partial class Progress : Form
	{
		private string documentName;

		public Progress( string documentName )
		{
			InitializeComponent();

			this.documentName = documentName;
		}

		public enum ProgressPhase
		{
			CreateEvidencePDF,
			SignEvidencePDF,
			FinalizingEvidencePDF
		}

		/// <summary>
		/// Sets the progress phase.
		/// </summary>
		/// <param name="progressPhase">Current phase.</param>
		public void SetPhase( ProgressPhase progressPhase )
		{
			if( this.InvokeRequired )
			{
				this.Invoke( ( MethodInvoker ) ( () => m_processing.Text = string.Format( GetProgressPhaseDescription( progressPhase ), this.documentName ) ) );
			}
			else
			{
				m_processing.Text = string.Format( GetProgressPhaseDescription( progressPhase ), this.documentName );	
			}
		}

		/// <summary>
		/// Closes this dialog safely from any thread.
		/// </summary>
		public void CloseFromAnyThread()
		{
			if( this.InvokeRequired )
			{
				this.Invoke( ( MethodInvoker )( this.Close ) );
			}
			else
			{
				this.Close();
			}
		}

		/// <summary>
		/// Gets the progress phase description as a formattable text.
		/// </summary>
		/// <param name="progressPhase">Progress phase</param>
		/// <returns>Progress description as a text.</returns>
		private static string GetProgressPhaseDescription( ProgressPhase progressPhase )
		{
			// Determine the text based on the progress phase.
			switch( progressPhase )
			{
			case ProgressPhase.CreateEvidencePDF:
				return InternalResources.ProgressPhase_CreateEvidencePDF;
			case ProgressPhase.SignEvidencePDF:
				return InternalResources.ProgressPhase_SignEvidencePDF;
			case ProgressPhase.FinalizingEvidencePDF:
				return InternalResources.ProgressPhase_FinalizingEvidencePDF;
			default:
				throw new System.Exception( string.Format( InternalResources.ProgressPhase_Unknown, progressPhase ) );
			}
		}
	}
}
