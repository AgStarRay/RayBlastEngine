namespace RayBlast; 

public abstract class Node {
	internal Node? parent = null;
	
	private readonly List<Node> children = new();
	
	protected void ChildrenUpdate() {
		foreach(Node child in children) {
			child.OnUpdate();
		}
	}
	
	protected void ChildrenRender() {
		foreach(Node child in children) {
			child.OnRender();
		}
	}
	
	protected virtual void OnUpdate() {
		ChildrenUpdate();
	}

	protected virtual void OnRender() {
		ChildrenRender();
	}
}
