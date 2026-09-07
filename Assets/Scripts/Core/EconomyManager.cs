using System;
using Underpin.SlotGame.Data;
using UnityEngine;

namespace Underpin.SlotGame.Core
{
    /// <summary>
    /// Manages player credits, bet increments, payouts, and free spin bonus counters.
    /// </summary>
    public class EconomyManager
    {
        private readonly PaytableConfig _config;
        private int _balance;
        private int _currentBetIndex;
        private int _freeSpinsRemaining;
        private int _freeSpinsTotalWon;

        // Events
        public event Action<int> OnBalanceChanged;
        public event Action<int, bool, bool> OnBetChanged; // (currentBet, isMinBet, isMaxBet)
        public event Action<int, int> OnFreeSpinsChanged; // (remaining, totalWon)

        public int Balance => _balance;
        public int CurrentBet => _config.BetAmounts[_currentBetIndex];
        public int CurrentBetIndex => _currentBetIndex;
        public int FreeSpinsRemaining => _freeSpinsRemaining;
        public int FreeSpinsTotalWon => _freeSpinsTotalWon;
        public bool IsInFreeSpins => _freeSpinsRemaining > 0;
        public bool IsMinBet => _currentBetIndex == 0;
        public bool IsMaxBet => _currentBetIndex == _config.BetAmounts.Length - 1;

        public EconomyManager(PaytableConfig config)
        {
            _config = config;
            _balance = config.StartingBalance;
            _currentBetIndex = config.DefaultBetIndex;
        }

        public void Initialize()
        {
            OnBalanceChanged?.Invoke(_balance);
            OnBetChanged?.Invoke(CurrentBet, IsMinBet, IsMaxBet);
            OnFreeSpinsChanged?.Invoke(_freeSpinsRemaining, _freeSpinsTotalWon);
        }

        public bool CanAffordSpin()
        {
            if (IsInFreeSpins) return true;
            return _balance >= CurrentBet;
        }

        public bool DeductBet()
        {
            if (IsInFreeSpins)
            {
                // Free spin does not deduct credits
                return true;
            }

            if (_balance >= CurrentBet)
            {
                _balance -= CurrentBet;
                OnBalanceChanged?.Invoke(_balance);
                return true;
            }

            return false;
        }

        public void AddPayout(int amount)
        {
            if (amount <= 0) return;

            _balance += amount;
            if (IsInFreeSpins)
            {
                _freeSpinsTotalWon += amount;
            }

            OnBalanceChanged?.Invoke(_balance);
            if (IsInFreeSpins)
            {
                OnFreeSpinsChanged?.Invoke(_freeSpinsRemaining, _freeSpinsTotalWon);
            }
        }

        public void IncreaseBet()
        {
            if (IsInFreeSpins) return;
            if (_currentBetIndex < _config.BetAmounts.Length - 1)
            {
                _currentBetIndex++;
                OnBetChanged?.Invoke(CurrentBet, IsMinBet, IsMaxBet);
            }
        }

        public void DecreaseBet()
        {
            if (IsInFreeSpins) return;
            if (_currentBetIndex > 0)
            {
                _currentBetIndex--;
                OnBetChanged?.Invoke(CurrentBet, IsMinBet, IsMaxBet);
            }
        }

        public void SetMaxBet()
        {
            if (IsInFreeSpins) return;
            _currentBetIndex = _config.BetAmounts.Length - 1;
            OnBetChanged?.Invoke(CurrentBet, IsMinBet, IsMaxBet);
        }

        public void AwardFreeSpins(int count)
        {
            _freeSpinsRemaining += count;
            OnFreeSpinsChanged?.Invoke(_freeSpinsRemaining, _freeSpinsTotalWon);
        }

        public void ConsumeFreeSpin()
        {
            if (_freeSpinsRemaining > 0)
            {
                _freeSpinsRemaining--;
                OnFreeSpinsChanged?.Invoke(_freeSpinsRemaining, _freeSpinsTotalWon);
            }
        }

        public void ResetFreeSpinsSession()
        {
            _freeSpinsRemaining = 0;
            _freeSpinsTotalWon = 0;
            OnFreeSpinsChanged?.Invoke(_freeSpinsRemaining, _freeSpinsTotalWon);
        }

        public void ResetBalance(int amount = -1)
        {
            _balance = amount > 0 ? amount : _config.StartingBalance;
            OnBalanceChanged?.Invoke(_balance);
        }
    }
}
