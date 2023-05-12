using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HelloMAUI.Models;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System.Collections.ObjectModel;
using System.Windows.Input;
using UraniumUI.Icons.MaterialIcons;

namespace HelloMAUI.ViewModels
{
    internal class NotesViewModel : ObservableObject, IQueryAttributable
    {
        public ObservableCollection<NoteViewModel> AllNotes { get; }
        public ICommand NewCommand { get; }
        public ICommand SelectNoteCommand { get; }
        public ICommand ChangeThemeCommand { get; }
        public FontImageSource ThemeImageSource { get; private set; }

        public NotesViewModel()
        {
            AllNotes = new ObservableCollection<NoteViewModel>(Note.LoadAll().Select(n => new NoteViewModel(n)));
            NewCommand = new AsyncRelayCommand(NewNoteAsync);
            SelectNoteCommand = new AsyncRelayCommand<NoteViewModel>(SelectNoteAsync);
            ChangeThemeCommand = new RelayCommand(ChangeTheme);

            ThemeImageSource = new FontImageSource();
            ThemeImageSource.FontFamily = nameof(MaterialRegular);
            ThemeImageSource.Glyph = GetCurrentTheme() == AppTheme.Light ? MaterialRegular.Dark_mode : MaterialRegular.Light_mode;
        }

        private void ChangeTheme()
        {
            var newTheme = GetCurrentTheme() == AppTheme.Light ? AppTheme.Dark : AppTheme.Light;

            Application.Current.UserAppTheme = newTheme;

            ThemeImageSource = new FontImageSource();
            ThemeImageSource.FontFamily = nameof(MaterialRegular);
            ThemeImageSource.Glyph = newTheme == AppTheme.Light ? MaterialRegular.Dark_mode : MaterialRegular.Light_mode;
            OnPropertyChanged(nameof(ThemeImageSource));
        }

        private AppTheme GetCurrentTheme()
        {
            AppTheme currentTheme = Application.Current.UserAppTheme == AppTheme.Unspecified ?
                Application.Current.PlatformAppTheme :
                Application.Current.UserAppTheme;
            return currentTheme;
        }

        private async Task NewNoteAsync()
        {
            await Shell.Current.GoToAsync(nameof(Views.NotePage));
        }

        private async Task SelectNoteAsync(NoteViewModel note)
        {
            if (note != null)
                await Shell.Current.GoToAsync($"{nameof(Views.NotePage)}?load={note.Identifier}");
        }

        void IQueryAttributable.ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("deleted"))
            {
                string noteId = query["deleted"].ToString();
                NoteViewModel matchedNote = AllNotes.Where((n) => n.Identifier == noteId).FirstOrDefault();

                // If note exists, delete it
                if (matchedNote != null)
                    AllNotes.Remove(matchedNote);
            }
            else if (query.ContainsKey("saved"))
            {
                string noteId = query["saved"].ToString();
                NoteViewModel matchedNote = AllNotes.Where((n) => n.Identifier == noteId).FirstOrDefault();

                // If note is found, update it
                if (matchedNote != null)
                {
                    matchedNote.Reload();
                    AllNotes.Move(AllNotes.IndexOf(matchedNote), 0);
                }

                // If note isn't found, it's new; add it.
                else
                    AllNotes.Insert(0, new NoteViewModel(Note.Load(noteId)));
            }
        }
    }
}
