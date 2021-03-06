﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Infrastructure.Orleans;
using Squidex.Infrastructure.States;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Indexes
{
    public class AppsByNameIndexGrainTests
    {
        private readonly IStore<string> store = A.Fake<IStore<string>>();
        private readonly IPersistence<AppsByNameIndexGrain.GrainState> persistence = A.Fake<IPersistence<AppsByNameIndexGrain.GrainState>>();
        private readonly Guid appId1 = Guid.NewGuid();
        private readonly Guid appId2 = Guid.NewGuid();
        private readonly string appName1 = "my-app1";
        private readonly string appName2 = "my-app2";
        private readonly AppsByNameIndexGrain sut;

        public AppsByNameIndexGrainTests()
        {
            A.CallTo(() => store.WithSnapshots(typeof(AppsByNameIndexGrain), SingleGrain.Id, A<HandleSnapshot<AppsByNameIndexGrain.GrainState>>.Ignored))
                .Returns(persistence);

            sut = new AppsByNameIndexGrain(store);
            sut.ActivateAsync(SingleGrain.Id).Wait();
        }

        [Fact]
        public async Task Should_add_app_id_to_index()
        {
            await sut.AddAppAsync(appId1, appName1);

            var result = await sut.GetAppIdAsync(appName1);

            Assert.Equal(appId1, result);

            A.CallTo(() => persistence.WriteSnapshotAsync(A<AppsByNameIndexGrain.GrainState>.Ignored))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_be_able_to_reserve_index_if_name_taken()
        {
            await sut.AddAppAsync(appId2, appName1);

            Assert.False(await sut.ReserveAppAsync(appId1, appName1));
        }

        [Fact]
        public async Task Should_not_be_able_to_reserve_if_name_reserved()
        {
            await sut.ReserveAppAsync(appId2, appName1);

            Assert.False(await sut.ReserveAppAsync(appId1, appName1));
        }

        [Fact]
        public async Task Should_not_be_able_to_reserve_if_id_taken()
        {
            await sut.AddAppAsync(appId1, appName1);

            Assert.False(await sut.ReserveAppAsync(appId1, appName2));
        }

        [Fact]
        public async Task Should_not_be_able_to_reserve_if_id_reserved()
        {
            await sut.ReserveAppAsync(appId1, appName1);

            Assert.False(await sut.ReserveAppAsync(appId1, appName2));
        }

        [Fact]
        public async Task Should_be_able_to_reserve_if_id_and_name_not_reserved()
        {
            await sut.ReserveAppAsync(appId1, appName1);

            Assert.True(await sut.ReserveAppAsync(appId2, appName2));
        }

        [Fact]
        public async Task Should_be_able_to_reserve_after_app_removed()
        {
            await sut.AddAppAsync(appId1, appName1);
            await sut.RemoveAppAsync(appId1);

            Assert.True(await sut.ReserveAppAsync(appId1, appName1));
        }

        [Fact]
        public async Task Should_be_able_to_reserve_after_reservation_removed()
        {
            await sut.ReserveAppAsync(appId1, appName1);
            await sut.RemoveReservationAsync(appId1, appName1);

            Assert.True(await sut.ReserveAppAsync(appId1, appName1));
        }

        [Fact]
        public async Task Should_return_many_app_ids()
        {
            await sut.AddAppAsync(appId1, appName1);
            await sut.AddAppAsync(appId2, appName2);

            var ids = await sut.GetAppIdsAsync(appName1, appName2);

            Assert.Equal(new List<Guid> { appId1, appId2 }, ids);
        }

        [Fact]
        public async Task Should_remove_app_id_from_index()
        {
            await sut.AddAppAsync(appId1, appName1);
            await sut.RemoveAppAsync(appId1);

            var result = await sut.GetAppIdAsync(appName1);

            Assert.Equal(Guid.Empty, result);

            A.CallTo(() => persistence.WriteSnapshotAsync(A<AppsByNameIndexGrain.GrainState>.Ignored))
                .MustHaveHappenedTwiceExactly();
        }

        [Fact]
        public async Task Should_replace_app_ids_on_rebuild()
        {
            var state = new Dictionary<string, Guid>
            {
                [appName1] = appId1,
                [appName2] = appId2
            };

            await sut.RebuildAsync(state);

            Assert.Equal(appId1, await sut.GetAppIdAsync(appName1));
            Assert.Equal(appId2, await sut.GetAppIdAsync(appName2));

            Assert.Equal(new List<Guid> { appId1, appId2 }, await sut.GetAppIdsAsync());

            Assert.Equal(2, await sut.CountAsync());

            A.CallTo(() => persistence.WriteSnapshotAsync(A<AppsByNameIndexGrain.GrainState>.Ignored))
                .MustHaveHappened();
        }
    }
}
