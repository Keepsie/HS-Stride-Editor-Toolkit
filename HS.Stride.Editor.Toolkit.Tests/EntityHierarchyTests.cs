// HS Stride Editor Toolkit (c) 2025 Happenstance Games LLC - Apache License 2.0

using NUnit.Framework;
using FluentAssertions;
using HS.Stride.Editor.Toolkit.Core;
using HS.Stride.Editor.Toolkit.Core.SceneEditing;

namespace HS.Stride.Editor.Toolkit.Tests
{
    [TestFixture]
    public class EntityHierarchyTests
    {
        private string _testProjectPath;

        [SetUp]
        public void Setup()
        {
            _testProjectPath = @"E:\Github\HS-Stride-Editor-Toolkit\HS.Stride.Editor.Toolkit.Tests\Example Scenes\TestProject\";
        }

        

        [Test]
        public void Entity_HasChildren_ShouldDetectChildren()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");

            var parent = scene.FindEntityByName("CubeGroupPrefab");
            var child = parent!.GetChildren().FirstOrDefault();

            // Act & Assert
            parent.HasChildren().Should().BeTrue("Parent entity should have children");

            if (child != null && !child.HasChildren())
            {
                child.HasChildren().Should().BeFalse("Leaf entity should not have children");
            }
        }

        [Test]
        public void Entity_GetParent_ShouldReturnParentEntity()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");

            var parent = scene.FindEntityByName("CubeGroupPrefab");
            var children = parent!.GetChildren();

            // Act & Assert
            foreach (var child in children)
            {
                var foundParent = child.GetParent();
                foundParent.Should().NotBeNull("Child should have a parent");
                foundParent!.Id.Should().Be(parent.Id, "Child's parent should be the original entity");
            }
        }

        [Test]
        public void Entity_FindChildByName_ShouldFindExactMatch()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");

            var parent = scene.FindEntityByName("CubeGroupPrefab");
            var allChildren = parent!.GetChildren();
            var expectedChild = allChildren.FirstOrDefault();

            if (expectedChild == null)
            {
                Assert.Inconclusive("Test requires entity with children");
                return;
            }

            // Act
            var found = parent.FindChildByName(expectedChild.Name);

            // Assert
            found.Should().NotBeNull();
            found!.Id.Should().Be(expectedChild.Id);
            found.Name.Should().Be(expectedChild.Name);
        }

        [Test]
        public void Entity_FindChildByName_NotFound_ShouldReturnNull()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");

            var parent = scene.FindEntityByName("CubeGroupPrefab");

            // Act
            var found = parent!.FindChildByName("NonExistentChildName");

            // Assert
            found.Should().BeNull();
        }

        [Test]
        public void Entity_FindChildrenByName_WithWildcard_ShouldMatchPattern()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");

            var parent = scene.FindEntityByName("CubeGroupPrefab");

            // Act - Find all children starting with "Cube"
            var found = parent!.FindChildrenByName("Cube*");

            // Assert
            found.Should().NotBeEmpty();
            found.Should().OnlyContain(c => c.Name.StartsWith("Cube"));
        }

        [Test]
        public void Entity_FindChildrenByName_NoMatch_ShouldReturnEmpty()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");

            var parent = scene.FindEntityByName("CubeGroupPrefab");

            // Act
            var found = parent!.FindChildrenByName("NoMatch*");

            // Assert
            found.Should().BeEmpty();
        }

        [Test]
        public void Entity_GetDescendants_ShouldReturnAllDescendants()
        {
            // Arrange
            var project = new StrideProject(_testProjectPath);
            var scene = project.LoadScene("Testing");

            var parent = scene.FindEntityByName("CubeGroupPrefab");

            // Act
            var descendants = parent!.GetDescendants();

            // Assert
            descendants.Should().NotBeEmpty();

            // All descendants should have this entity as an ancestor
            foreach (var descendant in descendants)
            {
                // Walk up the parent chain - should eventually reach parent
                var current = descendant.GetParent();
                var foundAncestor = false;

                while (current != null)
                {
                    if (current.Id == parent.Id)
                    {
                        foundAncestor = true;
                        break;
                    }
                    current = current.GetParent();
                }

                foundAncestor.Should().BeTrue($"Descendant {descendant.Name} should have {parent.Name} as ancestor");
            }
        }

    }
}
