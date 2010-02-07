using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TwitterLib;
using Witty.Controls.Search;
using WittyUnitTests.Search;

namespace SearchViewModel
{    
    [TestFixture]
    public class when_adding_a_valid_search_word : viewmodel_test_context 
    {
        [Test]
        public void it_puts_the_word_in_the_the_word_list()
        {
            _model.CurrentWord = _nominalSearchWord;
            _model.AddCurrentWordAsSearchTerm();

            Assert.That(_model.SearchItems.Single().Phrase == _nominalSearchWord);
        }

        [Test]
        public void it_ignores_duplicates()
        {

            _model.CurrentWord = _nominalSearchWord;
            _model.AddCurrentWordAsSearchTerm();

            _model.CurrentWord = _nominalSearchWord;
            _model.AddCurrentWordAsSearchTerm();

            Assert.That(_model.SearchItems.Count() == 1);
        }

        [Test]
        public void it_empties_the_CurrentWord()
        {
            _model.CurrentWord = _nominalSearchWord;
            _model.AddCurrentWordAsSearchTerm();

            Assert.That(_model.CurrentWord == String.Empty);
        }

        [Test]
        public void it_searches_for_the_new_word()
        {
            _model.CurrentWord = _nominalSearchWord;
            _model.AddCurrentWordAsSearchTerm();

            Assert.That(_searchService.LastTermSeen == _nominalSearchWord);
        }

        [Test]
        public void it_puts_search_results_in_viewmodel()
        {
            _model.CurrentWord = _nominalSearchWord;
            _model.AddCurrentWordAsSearchTerm();

            Assert.That(_model.SearchResults.Count() > 1);
        }

        [Test]
        public void it_associates_searchresults_with_searchitems()
        {
            _model.CurrentWord = _nominalSearchWord;
            _model.AddCurrentWordAsSearchTerm();

            Assert.That(_model.SearchResults.First().AssociatedSearchItem == 
                        _model.SearchItems.First());
        }

        [Test]
        public void it_includes_required_information_in_tweet()
        {
            _model.CurrentWord = _nominalSearchWord;
            _model.AddCurrentWordAsSearchTerm();

            Assert.That(_model.SearchResults.Count() == 15);
            var result = _model.SearchResults.First();            
            Assert.That(result.User != null);
            Assert.That(result.User.ImageUrl.EndsWith("scott_allen_2_normal.jpg"));
            Assert.That(result.User.ScreenName == "OdeToCode");
            Assert.That(result.Text.Contains("brightest moon of 2010"));
            Assert.That(result.RelativeTime != null);
            Assert.That(result.DateCreated != null);
            Assert.That(result.Source.Contains("seesmic"));                        
        }
    }

    [TestFixture]
    public class when_adding_invalid_search_words : viewmodel_test_context
    {
        [Test]
        public void the_words_do_not_appear_in_the_the_word_list()
        {
            _model.CurrentWord = null;
            _model.AddCurrentWordAsSearchTerm();

            _model.CurrentWord = "";
            _model.AddCurrentWordAsSearchTerm();

            Assert.That(_model.SearchItems.Count() == 0);
        }
    }

    [TestFixture]
    public class when_deleteing_a_searchword : viewmodel_test_context 
    {
        [Test]
        public void the_word_is_removed_from_the_list()
        {
            _model.CurrentWord = "bird";
            _model.AddCurrentWordAsSearchTerm();

            _model.CurrentWord = "blue";
            _model.AddCurrentWordAsSearchTerm();


            _model.DeleteTerm(_model.SearchItems.First());

            Assert.That(_model.SearchItems.Count() == 1);
        }

        [Test]
        public void it_removes_search_results_for_the_word()
        {
            _model.CurrentWord = _nominalSearchWord;
            _model.AddCurrentWordAsSearchTerm();
            _model.DeleteTerm(_model.SearchItems.First());
            
            Assert.That(_model.SearchResults.Count() == 0);
        }
    }
   
    [TestFixture]
    public class when_adding_search_results : viewmodel_test_context
    {
        [Test]
        public void it_limits_results_to_the_specified_amount()
        {
            var results = new List<SearchResult>();

            for (int i = 0; i < _maxSearchResults + 1; i++)
            {
                results.Add(
                        new SearchResult()
                        {
                            AssociatedSearchItem = new SearchItem(),
                            Text = i.ToString(),
                            User = new User
                            {
                                Name = "foo"
                            }                       
                        });
            }
            _model.SearchResults.AddResults(results);

            Assert.That(_model.SearchResults.Count() == _maxSearchResults);
        }

        [Test]
        public void it_prevents_duplicates()
        {
            var results = new List<SearchResult>();

            for(int i = 0; i < 10; i++)
            {
                results.Add(
                        new SearchResult()
                        {
                            AssociatedSearchItem = new SearchItem(),
                            Text = "foo",
                            User = new User
                            {
                                Name="foo"
                            }
                        });
            }

            _model.SearchResults.AddResults(results);

            Assert.That(_model.SearchResults.Count() == 1);
        }

        [Test]
        public void it_keeps_most_recent_result_first()
        {
            var result1 = new SearchResult()
            {
                AssociatedSearchItem = new SearchItem(),
                DateCreated = new DateTime(2007,1,1),
                Text = "old",
                User = new User
                {
                    Name="foo"
                }
            };

            _model.SearchResults.AddResults(Enumerable.Repeat(result1,1).ToList());

            var result2 = new SearchResult()
            {
                AssociatedSearchItem = new SearchItem(),
                DateCreated = new DateTime(2010,1,1),
                Text = "new",
                User = new User
                {
                    Name="foo"
                }
            };

            _model.SearchResults.AddResults(Enumerable.Repeat(result2,1).ToList());

            Assert.That(_model.SearchResults.First().Text == "new");
        }

    }

    [TestFixture]
    public class when_it_is_time_to_search : viewmodel_test_context 
    { 
        [Test]
        public void it_searches_for_an_overdue_term()
        {
            _timer.RaiseTickEvent();

            Assert.That(_searchService.LastTermSeen == _nominalSearchWord);
        }

        [Test]
        public void it_does_not_search_for_underdue_terms()
        {
            _model.SearchItems.First().LastSearch = _now.AddSeconds(1);
            _timer.RaiseTickEvent();

            Assert.That(_searchService.LastTermSeen == null);
        }

        [Test]
        public void it_sets_time_of_last_search_on_searchterm()
        {
            _timer.RaiseTickEvent();

            Assert.That(_model.SearchItems.First().LastSearch == _now);
        }

        public override void Setup()
        {            
            base.Setup();
            _model.CurrentWord = _nominalSearchWord;
            _model.AddCurrentWordAsSearchTerm();
            _searchService.Reset();
            _model.SearchItems.First().LastSearch = _now.AddDays(-1);
        }
    } 

    public class viewmodel_test_context
    {
        [SetUp]
        public virtual void Setup()
        {
            SystemTime.Now = () => _now;
            _searchService = new FakeTwitterSearchService();
            _timer = new FakeSearchTimer();
            var configuration = new SearchConfiguration()
            {
                MinSecondsBetweenItemSearch = 300,
                MaxSearchResults = _maxSearchResults,
                SearchService = _searchService,
                Timer = _timer
            };
            _model = new TweetHuntViewModel(configuration);
        }

        protected TweetHuntViewModel _model;
        protected string _nominalSearchWord = "poonam";
        protected FakeTwitterSearchService _searchService;
        protected FakeSearchTimer _timer;
        protected int _maxSearchResults = 20;
        protected DateTime _now = new DateTime(2010,2,1);
    }

}
