using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AdminClient.Management.Rooms
{
    public partial class Room : ObservableObject
    {
        [ObservableProperty]
        private string name = "";

        [ObservableProperty]
        private string containerName = "";

        [ObservableProperty]
        private int activeUsers = 0;

        [ObservableProperty]
        private int capacity;

        [RelayCommand]
        public async void DropRoom()
        {
            RoomsModel.Instance.DropRoom(this);
        }
    }

    public partial class RoomsModel : ObservableObject
    {
        public static RoomsModel Instance;

        [ObservableProperty]
        private ObservableCollection<Room> rooms = new ObservableCollection<Room>();

        [ObservableProperty]
        private string newRoomName = "";

        [ObservableProperty]
        private string newContainerName = "";

        [ObservableProperty]
        private int newRoomCapacity = 10;

        public void Init()
        {
            Instance = this;

            var rooms = RoomManagerClient.GetRooms();
            if(rooms != null)
            {
                foreach (var room in rooms)
                {
                    var r = new Room();
                    r.Name = room.Name;
                    r.ContainerName = room.ContainerName;
                    r.Capacity = room.Capacity;
                    r.ActiveUsers = room.ActiveUsers;

                    Rooms.Add(r);
                }
            }
        }

        [RelayCommand]
        public async void Refresh()
        {
            Init();
        }

        [RelayCommand]
        public async void DeployRoom()
        {
            _ = RoomManagerClient.DeployRoom(newContainerName, newRoomCapacity, newRoomName);
        }

        public async void DropRoom(Room room)
        {
            _ = RoomManagerClient.DropRoom(room.ContainerName);
            _ = rooms.Remove(room);
        }
    }
}
