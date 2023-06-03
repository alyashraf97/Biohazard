using Biohazard.Model;
using Moq;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Biohazard;
using Biohazard.Data;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using Microsoft.EntityFrameworkCore;

namespace Biohazard.Tests
{
    [TestClass]
    public class QMailRepositoryTests
    {
        [TestMethod]
        public async Task GetAllMailsAsync_ReturnsAllMails()
        {
            var mockContext = new Mock<QMailDbContext>();
            var mockMails = new List<QMail>{
                new QMail { Id = 1, Subject = "Test 1" },
                new QMail { Id = 2, Subject = "Test 2" }
            };

            var mockDbSet = new Mock<DbSet<QMail>>();
            IQueryable<QMail> queryableMails = (IQueryable<QMail>)mockMails;
            mockDbSet.As<IQueryable<QMail>>().Setup(m => m.Provider).Returns(queryableMails.Provider);
            mockDbSet.As<IQueryable<QMail>>().Setup(m => m.Expression).Returns(queryableMails.Expression);
            mockDbSet.As<IQueryable<QMail>>().Setup(m => m.ElementType).Returns(queryableMails.ElementType);
            mockDbSet.As<IQueryable<QMail>>().Setup(m => m.GetEnumerator()).Returns(mockMails.GetEnumerator());
            mockDbSet.Setup(m => m.Remove(It.IsAny<QMail>())).Callback((QMail mail) => mockMails.Remove(mail));
            mockContext.Setup(c => c.QMails).Returns(mockDbSet.Object);
            var repository = new QMailRepository(mockContext.Object);

            // Act
            var result = await repository.GetAllMailsAsync();

            // Assert
            Assert.AreEqual(2, result.Count());
            Assert.AreEqual("Test 1", result.First().Subject);
            Assert.AreEqual("Test 2", result.Last().Subject);
        }

        [TestMethod]
        public async Task GetMailByIdAsync_ReturnsMailById()
        {
            // Arrange
            var mockContext = new Mock<QMailDbContext>();
            var mockMails = new List<QMail>
            {
                new QMail { Id = 1, Subject = "Test 1" },
                new QMail { Id = 2, Subject = "Test 2" }
            }.AsQueryable();
            mockContext.Setup(c => c.QMails.FindAsync(1)).ReturnsAsync(mockMails.First());
            var repository = new QMailRepository(mockContext.Object);

            // Act
            var result = await repository.GetMailByIdAsync("1");

            // Assert
            Assert.AreEqual(1, result.Id);
            Assert.AreEqual("Test 1", result.Subject);
        }
        /*
        [TestMethod]
        public async Task AddMailAsync_AddsMailToDbSetAndSavesChanges()
        {
            // Arrange
            var mockContext = new Mock<QMailDbContext>();
            var mockMails = new List<QMail>().AsQueryable();
            mockContext.Setup(c => c.QMails).Returns(mockMails);
            mockContext.Setup(c => c.QMails.AddAsync(It.IsAny<QMail>())).Callback((QMail mail) => mockMails.Add(mail));
            mockContext.Setup(c => c.SaveChangesAsync()).ReturnsAsync(1);
            var repository = new QMailRepository(mockContext.Object);
            var newMail = new QMail { Id = 3, Subject = "Test 3" };

            // Act
            await repository.AddMailAsync(newMail);

            // Assert
            mockContext.Verify(c => c.QMails.AddAsync(newMail), Times.Once());
            mockContext.Verify(c => c.SaveChangesAsync(), Times.Once());
        }
        */
    }
}
