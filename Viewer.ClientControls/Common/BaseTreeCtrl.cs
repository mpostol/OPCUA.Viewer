//<summary>
//  Title   : Base tree 
//  System  : Microsoft Visual C# .NET 2008
//  $LastChangedDate$
//  $Rev$
//  $LastChangedBy$
//  $URL$
//  $Id$
//
//  Copyright (C)2009, CAS LODZ POLAND.
//  TEL: +48 (42) 686 25 47
//  mailto://techsupp@cas.eu
//  http://www.cas.eu
//</summary>

using System;
using System.Reflection;
using System.Windows.Forms;

namespace CAS.OPC.UA.Viewer.Client.Controls
{
  /// <summary>
  /// A base class for tree controls.
  /// </summary>
  public partial class BaseTreeCtrl : UserControl
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="BaseTreeCtrl"/> class.
    /// </summary>
    public BaseTreeCtrl()
    {
      InitializeComponent();
      NodesTV.ImageList = new GuiUtils().ImageList;
      NodesTV.ItemHeight = 18;
    }

    #region Public Interface
    /// <summary>
    /// The TreeView contained in the Control.
    /// </summary>
    public System.Windows.Forms.TreeView NodesTV;

    /// <summary>
    /// Raised whenever a node is 'picked' in the control.
    /// </summary>
    public event TreeNodeActionEventHandler NodePicked
    {
      add { m_NodePicked += value; }
      remove { m_NodePicked -= value; }
    }

    /// <summary>
    /// Raised whenever a node is selected in the control.
    /// </summary>
    public event TreeNodeActionEventHandler NodeSelected
    {
      add { m_NodeSelected += value; }
      remove { m_NodeSelected -= value; }
    }

    /// <summary>
    /// Whether the control should allow items to be dragged.
    /// </summary>
    public bool EnableDragging
    {
      get { return m_enableDragging; }
      set { m_enableDragging = value; }
    }
    #endregion

    #region Private Fields
    private event TreeNodeActionEventHandler m_NodePicked;
    private event TreeNodeActionEventHandler m_NodeSelected;
    private bool m_enableDragging;
    #endregion

    #region Protected Methods
    /// <summary>
    /// Adds an item to the tree.
    /// </summary>
    protected virtual TreeNode AddNode(TreeNode treeNode, object item)
    {
      return AddNode(treeNode, item, String.Format("{0}", item), "ClosedFolder");
    }

    /// <summary>
    /// Adds an item to the tree.
    /// </summary>
    protected virtual TreeNode AddNode(TreeNode parent, object item, string text, string icon)
    {
      // create node.
      TreeNode treeNode = new TreeNode();

      // update text/icon.
      UpdateNode(treeNode, item, text, icon);

      // add to control.
      if (parent == null)
      {
        NodesTV.Nodes.Add(treeNode);
      }
      else
      {
        parent.Nodes.Add(treeNode);
      }

      // return new tree node.
      return treeNode;
    }

    /// <summary>
    /// Updates a tree node with the current contents of an object.
    /// </summary>
    protected virtual void UpdateNode(TreeNode treeNode, object item, string text, string icon)
    {
      treeNode.Text = text;
      treeNode.Tag = item;
      treeNode.ImageKey = icon;
      treeNode.SelectedImageKey = icon;
    }

    /// <summary>
    /// Returns the data to drag.
    /// </summary>
    protected virtual object GetDataToDrag(TreeNode node)
    {
      return node.Tag;
    }

    /// <summary>
    /// Enables the state of menu items.
    /// </summary>
    protected virtual void EnableMenuItems(TreeNode clickedNode)
    {
      // do nothing.
    }

    /// <summary>
    /// Initializes a node before expanding it.
    /// </summary>
    protected virtual bool BeforeExpand(TreeNode clickedNode)
    {
      return false;
    }

    /// <summary>
    /// Sends notifications whenever a node in the control is 'picked'.
    /// </summary>
    protected virtual void PickNode()
    {
      if (m_NodePicked != null)
      {
        if (NodesTV.SelectedNode != null)
        {
          object parent = null;

          if (NodesTV.SelectedNode.Parent != null)
          {
            parent = NodesTV.SelectedNode.Tag;
          }

          m_NodePicked(this, new TreeNodeActionEventArgs(TreeNodeAction.Picked, NodesTV.SelectedNode.Tag, parent));
        }
      }
    }

    /// <summary>
    /// Sends notifications whenever a node in the control is 'selected'.
    /// </summary>
    protected virtual void SelectNode()
    {
      if (m_NodeSelected != null)
      {
        if (NodesTV.SelectedNode != null)
        {
          object parent = null;

          if (NodesTV.SelectedNode.Parent != null)
          {
            parent = NodesTV.SelectedNode.Tag;
          }

          m_NodeSelected(this, new TreeNodeActionEventArgs(TreeNodeAction.Selected, NodesTV.SelectedNode.Tag, parent));
        }
      }
    }

    /// <summary>
    /// Returns the Tag for the current selection.
    /// </summary>
    protected object SelectedTag
    {
      get
      {
        if (NodesTV.SelectedNode != null)
        {
          return NodesTV.SelectedNode.Tag;
        }

        return null;
      }
    }
    #endregion

    #region Event Handlers
    private void NodesTV_AfterSelect(object sender, TreeViewEventArgs e)
    {
      try
      {
        SelectNode();
      }
      catch (Exception exception)
      {
        GuiUtils.HandleException(this.Text, MethodBase.GetCurrentMethod(), exception);
      }
    }

    private void NodesTV_BeforeExpand(object sender, TreeViewCancelEventArgs e)
    {
      try
      {
        Cursor = Cursors.WaitCursor;
        e.Cancel = BeforeExpand(e.Node);
      }
      catch (Exception exception)
      {
        GuiUtils.HandleException(this.Text, MethodBase.GetCurrentMethod(), exception);
      }
      finally
      {
        Cursor = Cursors.Default;
      }
    }

    private void NodesTV_DoubleClick(object sender, EventArgs e)
    {
      try
      {
        PickNode();
      }
      catch (Exception exception)
      {
        GuiUtils.HandleException(this.Text, MethodBase.GetCurrentMethod(), exception);
      }
    }

    /// <summary>
    /// Handles the MouseDown event of the NodesTV control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
    public void NodesTV_MouseDown(object sender, MouseEventArgs e)
    {
      try
      {
        // selects the item that was right clicked on.
        TreeNode clickedNode = NodesTV.SelectedNode = NodesTV.GetNodeAt(e.X, e.Y);

        // no item clicked on - do nothing.
        if (clickedNode == null) return;

        // start drag operation.
        if (e.Button == MouseButtons.Left)
        {
          if (m_enableDragging)
          {
            object data = GetDataToDrag(clickedNode);

            if (data != null)
            {
              NodesTV.DoDragDrop(data, DragDropEffects.Copy);
            }
          }

          return;
        }

        // disable everything.
        if (NodesTV.ContextMenuStrip != null)
        {
          foreach (ToolStripItem item in NodesTV.ContextMenuStrip.Items)
          {
            ToolStripMenuItem menuItem = item as ToolStripMenuItem;

            if (menuItem == null)
            {
              continue;
            }

            menuItem.Enabled = false;

            if (menuItem.DropDown != null)
            {
              foreach (ToolStripItem subItem in menuItem.DropDown.Items)
              {
                ToolStripMenuItem subMenuItem = subItem as ToolStripMenuItem;

                if (subMenuItem != null)
                {
                  subMenuItem.Enabled = false;
                }
              }
            }
          }
        }

        // enable menu items according to context.
        if (e.Button == MouseButtons.Right)
        {
          EnableMenuItems(clickedNode);
        }
      }
      catch (Exception exception)
      {
        GuiUtils.HandleException(this.Text, MethodBase.GetCurrentMethod(), exception);
      }
    }

    /// <summary>
    /// Handles the DragEnter event of the NodesTV control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing the event data.</param>
    protected virtual void NodesTV_DragEnter(object sender, DragEventArgs e)
    {
      if (m_enableDragging)
      {
        e.Effect = DragDropEffects.Copy;
      }
      else
      {
        e.Effect = DragDropEffects.None;
      }
    }

    /// <summary>
    /// Handles the DragDrop event of the NodesTV control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing the event data.</param>
    protected virtual void NodesTV_DragDrop(object sender, DragEventArgs e)
    {
      // overridden by sub-class.
    }

    /// <summary>
    /// Handles the GiveFeedback event of the NodesTV control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.Forms.GiveFeedbackEventArgs"/> instance containing the event data.</param>
    protected virtual void NodesTV_GiveFeedback(object sender, GiveFeedbackEventArgs e)
    {
      // overridden by sub-class.
    }

    /// <summary>
    /// Handles the DragOver event of the NodesTV control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.Windows.Forms.DragEventArgs"/> instance containing the event data.</param>
    protected virtual void NodesTV_DragOver(object sender, DragEventArgs e)
    {
      // overridden by sub-class.
    }
    #endregion
  }

  #region TreeNodeAction Eumeration
  /// <summary>
  /// The possible actions that could affect a node.
  /// </summary>
  public enum TreeNodeAction
  {
    /// <summary>
    /// A node was picked in the tree.
    /// </summary>
    Picked,

    /// <summary>
    /// A node was selected in the tree.
    /// </summary>
    Selected
  }
  #endregion

  #region TreeNodeActionEventArgs class
  /// <summary>
  /// The event argurments passed when an node event occurs.
  /// </summary>
  public class TreeNodeActionEventArgs : EventArgs
  {
    #region Constructor
    /// <summary>
    /// Initializes the object.
    /// </summary>
    public TreeNodeActionEventArgs(TreeNodeAction action, object node, object parent)
    {
      m_node = node;
      m_parent = parent;
      m_action = action;
    }
    #endregion

    #region Private Fields
    /// <summary>
    /// The tag associated with the node that was acted on.
    /// </summary>
    public object Node
    {
      get { return m_node; }
    }

    /// <summary>
    /// The tag associated with the parent of the node that was acted on.
    /// </summary>
    public object Parent
    {
      get { return m_parent; }
    }

    /// <summary>
    /// The action in question.
    /// </summary>
    public TreeNodeAction Action
    {
      get { return m_action; }
    }
    #endregion

    #region Private Fields
    private object m_node;
    private object m_parent;
    private TreeNodeAction m_action;
    #endregion
  }

  /// <summary>
  /// The delegate used to receive node action events.
  /// </summary>
  public delegate void TreeNodeActionEventHandler(object sender, TreeNodeActionEventArgs e);
  #endregion
}
