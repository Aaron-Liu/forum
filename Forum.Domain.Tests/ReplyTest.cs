﻿using System.Threading;
using ECommon.Extensions;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Infrastructure;
using Forum.Commands.Accounts;
using Forum.Commands.Posts;
using Forum.Commands.Replies;
using NUnit.Framework;

namespace Forum.Domain.Tests
{
    [TestFixture]
    public class ReplyTest : TestBase
    {
        [Test]
        public void create_reply_test1()
        {
            //创建账号
            var name = ObjectId.GenerateNewStringId();
            var password = ObjectId.GenerateNewStringId();
            var result = ExecuteCommand(new RegisterNewAccountCommand(ObjectId.GenerateNewStringId(), name, password));
            Assert.AreEqual(CommandStatus.Success, result.Status);

            //发表帖子
            var authorId = result.AggregateRootId;
            var subject = ObjectId.GenerateNewStringId();
            var body = ObjectId.GenerateNewStringId();
            var sectionId = ObjectId.GenerateNewStringId();
            result = ExecuteCommand(new CreatePostCommand(ObjectId.GenerateNewStringId(), subject, body, sectionId, authorId));
            Assert.AreEqual(CommandStatus.Success, result.Status);
            Assert.IsNotNull(result.AggregateRootId);

            //发表回复
            var postId = result.AggregateRootId;
            result = ExecuteCommand(new CreateReplyCommand(ObjectId.GenerateNewStringId(), postId, null, body, authorId));

            //验证回复信息
            Assert.AreEqual(CommandStatus.Success, result.Status);
            Assert.IsNotNull(result.AggregateRootId);
            var replyId = result.AggregateRootId;
            var reply = _replyQueryService.FindDynamic(replyId, "simple");
            Assert.NotNull(reply);
            Assert.AreEqual(replyId, reply.id);
            Assert.AreEqual(postId, reply.postId);
            Assert.AreEqual(authorId, reply.authorId);
            Assert.AreEqual(body, reply.body);

            //停顿3s后验证帖子统计信息
            Thread.Sleep(3000);
            var postInfo = _postQueryService.Find(postId);
            Assert.NotNull(postInfo);
            Assert.AreEqual(replyId, postInfo.MostRecentReplyId);
            Assert.AreEqual(authorId, postInfo.MostRecentReplierId);
            Assert.AreEqual(name, postInfo.MostRecentReplierName);
            Assert.AreEqual(reply.createdOn, postInfo.LastUpdateTime);
            Assert.AreEqual(1, postInfo.ReplyCount);
        }

        [Test]
        public void create_reply_test2()
        {
            var postId = ObjectId.GenerateNewStringId();
            var authorId = ObjectId.GenerateNewStringId();
            var body = ObjectId.GenerateNewStringId();

            var id1 = ExecuteCommand(new CreateReplyCommand(ObjectId.GenerateNewStringId(), postId, null, body, authorId)).AggregateRootId;

            var body2 = ObjectId.GenerateNewStringId();

            var id2 = ExecuteCommand(new CreateReplyCommand(ObjectId.GenerateNewStringId(), postId, id1, body2, authorId)).AggregateRootId;

            var reply = _replyQueryService.FindDynamic(id2, "simple");

            Assert.NotNull(reply);
            Assert.AreEqual(id2, reply.id);
            Assert.AreEqual(postId, reply.postId);
            Assert.AreEqual(authorId, reply.authorId);
            Assert.AreEqual(id1, reply.parentId);
            Assert.AreEqual(body2, reply.body);
        }

        [Test]
        public void update_reply_body_test()
        {
            var postId = ObjectId.GenerateNewStringId();
            var authorId = ObjectId.GenerateNewStringId();
            var body = ObjectId.GenerateNewStringId();

            var id = ExecuteCommand(new CreateReplyCommand(ObjectId.GenerateNewStringId(), postId, null, body, authorId)).AggregateRootId;

            var body2 = ObjectId.GenerateNewStringId();

            ExecuteCommand(new ChangeReplyBodyCommand(id, body2));

            var reply = _replyQueryService.FindDynamic(id, "simple");

            Assert.NotNull(reply);
            Assert.AreEqual(id, reply.id);
            Assert.AreEqual(postId, reply.postId);
            Assert.AreEqual(authorId, reply.authorId);
            Assert.AreEqual(body2, reply.body);
        }
    }
}
