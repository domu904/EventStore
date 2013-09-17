﻿// Copyright (c) 2012, Event Store LLP
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
// 
// Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// Redistributions in binary form must reproduce the above copyright
// notice, this list of conditions and the following disclaimer in the
// documentation and/or other materials provided with the distribution.
// Neither the name of the Event Store LLP nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;

namespace EventStore.Projections.Core.Services.Processing
{
    public class ParallelProcessingLoadBalancer
    {
        private class State
        {
            public int UnmeasuredTasksScheduled;
            public long ScheduledSize;
        }

        private readonly int _workers;
        private readonly long _maxScheduledSizePerWorker;
        private readonly int _maxUnmeasuredTasksPerWorker;

        private readonly State[] _state;

        public ParallelProcessingLoadBalancer(
            int workers, long maxScheduledSizePerWorker, int maxUnmeasuredTasksPerWorker)
        {
            if (workers <= 0) throw new ArgumentException("At least one worker required", "workers");
            if (maxScheduledSizePerWorker <= 0) throw new ArgumentException("maxScheduledSizePerWorker <= 0");
            if (maxUnmeasuredTasksPerWorker <= 0) throw new ArgumentException("maxUnmeasuredTasksPerWorker <= 0");

            _workers = workers;
            _maxScheduledSizePerWorker = maxScheduledSizePerWorker;
            _maxUnmeasuredTasksPerWorker = maxUnmeasuredTasksPerWorker;

            _state = new State[workers];
            for (int index = 0; index < _state.Length; index++)
                _state[index] = new State();

        }

        public void AccountMeasured(object task, int size)
        {
            throw new NotImplementedException();
        }

        public void AccountCompleted(object task)
        {
            throw new NotImplementedException();
        }

        public void ScheduleTask<T>(T task, Action<T, int> scheduled)
        {
            var index = FindLeastLoaded();
            _state[index].UnmeasuredTasksScheduled ++;
            scheduled(task, index);
        }

        private int FindLeastLoaded()
        {
            var bestIndex = -1;
            var best = long.MaxValue;
            for (var i = 0; i < _state.Length; i++)
            {
                var current = EstimateWorkerLoad(i);
                if (current < best)
                {
                    best = current;
                    bestIndex = i;
                }
            }
            return bestIndex;
        }

        private long EstimateWorkerLoad(int i)
        {
            return _state[i].UnmeasuredTasksScheduled*10 + _state[i].ScheduledSize;
        }
    }
}