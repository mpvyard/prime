﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using Prime.Core;
using Prime.Core.Wallet;
using GalaSoft.MvvmLight.Command;
using Humanizer;

namespace Prime.Ui.Wpf.ViewModel
{
    public class PortfolioPaneViewModel : DocumentPaneViewModel
    {
        public PortfolioPaneViewModel() { }

        public PortfolioPaneViewModel(ScreenViewModel screenViewModel)
        {
            _context = UserContext.Current;
            Provider = _context.PortfolioProvider;
            Provider.OnChanged += Portfolio_OnChanged;
            Dispatcher = Application.Current.Dispatcher;
        }

        private readonly ScreenViewModel _screenViewModel;
        public readonly Dispatcher Dispatcher;
        private readonly UserContext _context;
        public readonly PortfolioProvider Provider;
        private DateTime _utcLastUpdated;
        private string _information;
        private Money _totalConverted;

        public event EventHandler OnChanged;

        public DateTime UtcLastUpdated
        {
            get => _utcLastUpdated;
            set => Set(ref _utcLastUpdated, value);
        }

        public Money TotalConverted
        {
            get => _totalConverted;
            set => Set(ref _totalConverted, value);
        }

        public string Information
        {
            get => _information;
            set => Set(ref _information, value);
        }

        public ObservableCollection<PortfolioGroupedItem> SummaryObservable { get; private set; } = new ObservableCollection<PortfolioGroupedItem>();

        public ObservableCollection<PortfolioLineItem> PortfolioObservable { get; private set; } = new ObservableCollection<PortfolioLineItem>();

        public ObservableCollection<PortfolioInfoItem> PortfolioInfoObservable { get; private set; } = new ObservableCollection<PortfolioInfoItem>();

        private void Portfolio_OnChanged(object sender, System.EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var p = Provider;

                PortfolioObservable.Clear();

                foreach (var i in p.OrderBy(x => x.Asset?.ShortCode ?? "ZZZ").ThenBy(x => x.Network?.Name ?? "ZZZ"))
                    PortfolioObservable.Add(i);

                PortfolioInfoObservable.Clear();

                foreach (var i in p.PortfolioInfoItems.OrderBy(x => x.Network.NameLowered))
                    PortfolioInfoObservable.Add(i);

                var t = PortfolioInfoObservable.Sum(x => x.TotalConvertedAssetValue);
                var r = t==0 ? 0 : 100 / t;
                foreach (var i in PortfolioInfoObservable)
                    i.Percentage = r * i.TotalConvertedAssetValue;

                var gi = new List<PortfolioGroupedItem>();
                foreach (var i in p.GroupBy(x => x.Asset))
                    gi.Add(PortfolioGroupedItem.Create(i.Key, i.ToList()));

                SummaryObservable.Clear();
                foreach (var i in gi.OrderBy(x => x.IsTotalLine).ThenByDescending(x => x.Converted))
                    SummaryObservable.Add(i);

                UtcLastUpdated = p.UtcLastUpdated;
                Information = null;

                Information += "Via: " + string.Join(", ", p.WorkingProviders.OrderBy(x => x.Network.Name).Select(x => x.Network.Name));

                if (p.FailingProviders.Any())
                    Information += " | Failed: " + string.Join(", ", p.FailingProviders.OrderBy(x => x.Network.Name).Select(x => x.Network.Name));

                TotalConverted = p.FirstOrDefault(x => x.IsTotalLine)?.Converted ?? Money.Zero;

                if (p.Any(x=>x.ConversionFailed))
                    Information += " Warning: Some currencies could not be converted, and so are not included in this total.";

                OnChanged?.Invoke(this, EventArgs.Empty);

                RaisePropertyChanged(nameof(UtcLastUpdated));
                RaisePropertyChanged(nameof(TotalConverted));
                RaisePropertyChanged(nameof(Information));
                RaisePropertyChanged(nameof(SummaryObservable));
                RaisePropertyChanged(nameof(PortfolioObservable));
                RaisePropertyChanged(nameof(PortfolioInfoObservable));
            });
        }

        public override CommandContent Create()
        {
            return new SimpleContentCommand("portfolio");
        }
    }
}