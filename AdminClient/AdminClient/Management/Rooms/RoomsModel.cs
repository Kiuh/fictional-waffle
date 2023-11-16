using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace AdminClient.Management.Rooms
{
    public partial class Room: ObservableObject
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

    public partial class RoomsModel: ObservableObject
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

            var r = new Room();
            r.Name = "test_room";
            r.ContainerName = "room_1067";
            r.Capacity = 10;
            r.ActiveUsers = 0;
            rooms.Add(r);
            r = new Room();
            r.Name = "test_room_2";
            r.ContainerName = "room_1132";
            r.Capacity = 15;
            r.ActiveUsers = 0;
            rooms.Add(r);
            r = new Room();
            r.Name = "test_room_3";
            r.ContainerName = "room_671";
            r.Capacity = 5;
            r.ActiveUsers = 0;
            rooms.Add(r);

            //var rooms = RoomManagerClient.GetRooms().Result;
            //foreach (var room in rooms)
            //{
            //    var r = new Room();
            //    r.Name = room.Name;
            //    r.ContainerName = room.ContainerName;
            //    r.Capacity = room.Capacity;
            //    r.ActiveUsers = room.ActiveUsers;

            //    Rooms.Add(r);
            //}
        }

        [RelayCommand]
        public async void Refresh()
        {
            Init();
        }

        [RelayCommand]
        public async void DeployRoom()
        {
            RoomManagerClient.DeployRoom(newContainerName, newRoomCapacity, newRoomName);
        }

        public async void DropRoom(Room room)
        {
            RoomManagerClient.DropRoom(room.ContainerName);
            rooms.Remove(room);
        }
    }
}
