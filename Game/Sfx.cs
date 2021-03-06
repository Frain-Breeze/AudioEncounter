using System.Collections.Generic;

using Godot;

using static Assert;



public enum SfxCatagory {
	FALL_CRUNCH,
	EMPTY_CHAMBER_FIRE_CLICK,
	RELOAD,
	BULLET_HIT,
	FLESH_HIT,
	PISTOL_FIRE,
	AK_FIRE,
	SHOTGUN_FIRE,
	CASING_TINK,
	DRIP,
	CONCRETE_FOOTSTEPS,
	LEAVES_FOOTSTEPS,
	METAL_FOOTSTEPS,
	MARBLE_FOOTSTEPS,
};



public class Sfx2DCleanup : AudioStreamPlayer {
	public override void _Ready() {
		Connect("finished", this, nameof(OnPlaybackFinished));
		base._Ready();
	}


	public void OnPlaybackFinished() {
		QueueFree();
	}
}



public class Sfx3DCleanup : AudioStreamPlayer3D {
	public override void _Ready() {
		Connect("finished", this, nameof(OnPlaybackFinished));
		base._Ready();
	}


	public void OnPlaybackFinished() {
		QueueFree();
	}
}



public class Sfx : Node {
	public const bool PlayLocally = true;
	public const bool PlayRemote = true;

	public static Sfx Self;

	public static Dictionary<SfxCatagory, List<AudioStream>> Clips = new Dictionary<SfxCatagory, List<AudioStream>>();


	static Sfx() {
		if(Engine.EditorHint) {
			return;
		}

		Clips.Add(SfxCatagory.FALL_CRUNCH, new List<AudioStream> {
			GD.Load<AudioStream>("res://TrimmedAudio/FallCrunch.wav")
		});
		Clips.Add(SfxCatagory.EMPTY_CHAMBER_FIRE_CLICK, new List<AudioStream> {
			GD.Load<AudioStream>("res://TrimmedAudio/EmptyChamberFireClick.wav")
		});
		Clips.Add(SfxCatagory.RELOAD, new List<AudioStream> {
			GD.Load<AudioStream>("res://TrimmedAudio/ReloadStart.wav"),
			GD.Load<AudioStream>("res://TrimmedAudio/ReloadEnd.wav")
		});
		Clips.Add(SfxCatagory.BULLET_HIT, new List<AudioStream> {
			GD.Load<AudioStream>("res://TrimmedAudio/BulletHit.wav")
		});
		Clips.Add(SfxCatagory.FLESH_HIT, new List<AudioStream> {
			GD.Load<AudioStream>("res://TrimmedAudio/FleshHit.wav")
		});

		Clips.Add(SfxCatagory.PISTOL_FIRE, new List<AudioStream> {
			GD.Load<AudioStream>("res://TrimmedAudio/PistolFire.wav")
		});
		Clips.Add(SfxCatagory.AK_FIRE, new List<AudioStream> {
			GD.Load<AudioStream>("res://TrimmedAudio/AkFire.wav")
		});
		Clips.Add(SfxCatagory.SHOTGUN_FIRE, new List<AudioStream> {
			GD.Load<AudioStream>("res://TrimmedAudio/ShotgunFire.wav")
		});

		List<AudioStream> CasingTinks = LoadAllStreamsInFolder("res://TrimmedAudio/CasingTinks");
		Clips.Add(SfxCatagory.CASING_TINK, CasingTinks);

		List<AudioStream> Drips = LoadAllStreamsInFolder("res://TrimmedAudio/Drips");
		Clips.Add(SfxCatagory.DRIP, Drips);

		List<AudioStream> ConcreteFootsteps = LoadAllStreamsInFolder("res://TrimmedAudio/ConcreteFootsteps");
		Clips.Add(SfxCatagory.CONCRETE_FOOTSTEPS, ConcreteFootsteps);

		List<AudioStream> LeavesFootsteps = LoadAllStreamsInFolder("res://TrimmedAudio/LeavesFootsteps");
		Clips.Add(SfxCatagory.LEAVES_FOOTSTEPS, LeavesFootsteps);

		List<AudioStream> MetalFootsteps = LoadAllStreamsInFolder("res://TrimmedAudio/MetalFootsteps");
		Clips.Add(SfxCatagory.METAL_FOOTSTEPS, MetalFootsteps);

		List<AudioStream> MarbleFootsteps = LoadAllStreamsInFolder("res://TrimmedAudio/MarbleFootsteps");
		Clips.Add(SfxCatagory.MARBLE_FOOTSTEPS, MarbleFootsteps);
	}


	public static List<AudioStream> LoadAllStreamsInFolder(string Path) {
		var Output = new List<AudioStream>();

		var Dir = new Directory();
		ActualAssert(Dir.Open(Path) == Error.Ok);

		Dir.ListDirBegin();

		string FileName = Dir.GetNext();
		while(FileName.Length > 0) {
			if(!Dir.CurrentIsDir()) {
				if(!OS.IsDebugBuild() && FileName.EndsWith(".import")) {
					var Stream = GD.Load<AudioStream>($"{Path}/{FileName.Replace(".import", "")}");
					ActualAssert(Stream != null);
					Output.Add(Stream);
				}
				else if(OS.IsDebugBuild() && !FileName.EndsWith(".import")) {
					var Stream = GD.Load<AudioStream>($"{Path}/{FileName}");
					ActualAssert(Stream != null);
					Output.Add(Stream);
				}
			}

			FileName = Dir.GetNext();
		}

		Dir.ListDirEnd();

		return Output;
	}


	public override void _Ready() {
		if(Engine.EditorHint) {
			return;
		}

		Self = this;
		base._Ready();
	}


	public static void PlaySfx(SfxCatagory Catagory, int Index, Vector3 Pos, float Muffle) {
		if(PlayLocally) {
			PlaySfxLocally(Catagory, Index, Muffle);
		}

		if(PlayRemote) {
			Self.Rpc(nameof(PlaySfxAt), Catagory, Index, Pos, Muffle);
		}
	}


	public static void PlaySfxSpatially(SfxCatagory Catagory, int Index, Vector3 Pos, float Muffle) {
		if(PlayLocally) {
			Self.PlaySfxAt(Catagory, Index, Pos, Muffle);
		}

		if(PlayRemote) {
			Self.Rpc(nameof(PlaySfxAt), Catagory, Index, Pos, Muffle);
		}
	}


	private static void PlaySfxLocally(SfxCatagory Catagory, int Index, float Muffle) {
		Sfx2DCleanup StreamPlayer = new Sfx2DCleanup();
		StreamPlayer.Stream = Clips[Catagory][Index];
		StreamPlayer.Bus = "Reverb";

		switch(Catagory) {
			case SfxCatagory.EMPTY_CHAMBER_FIRE_CLICK: {
				StreamPlayer.VolumeDb = -8;
				break;
			}

			case SfxCatagory.PISTOL_FIRE: {
				StreamPlayer.VolumeDb = -6;
				break;
			}

			case SfxCatagory.AK_FIRE: {
				StreamPlayer.VolumeDb = -6;
				break;
			}

			case SfxCatagory.SHOTGUN_FIRE: {
				StreamPlayer.VolumeDb = 0;
				break;
			}

			case SfxCatagory.CONCRETE_FOOTSTEPS: {
				StreamPlayer.VolumeDb = 6;
				break;
			}

			case SfxCatagory.LEAVES_FOOTSTEPS: {
				StreamPlayer.VolumeDb = -5;
				break;
			}

			case SfxCatagory.METAL_FOOTSTEPS: {
				StreamPlayer.VolumeDb = -5;
				break;
			}

			case SfxCatagory.MARBLE_FOOTSTEPS: {
				StreamPlayer.VolumeDb = -10;
				break;
			}
		}

		StreamPlayer.VolumeDb -= Muffle / 1.5f;

		Game.RuntimeRoot.AddChild(StreamPlayer);
		StreamPlayer.Play();
	}


	[Remote]
	private void PlaySfxAt(SfxCatagory Catagory, int Index, Vector3 Pos, float Muffle) {
		Sfx3DCleanup StreamPlayer = new Sfx3DCleanup();
		StreamPlayer.Stream = Clips[Catagory][Index];
		StreamPlayer.Bus = "Reverb";

		switch(Catagory) {
			case SfxCatagory.FALL_CRUNCH: {
				StreamPlayer.UnitDb = 1;
				StreamPlayer.UnitSize = 18;
				break;
			}

			case SfxCatagory.EMPTY_CHAMBER_FIRE_CLICK: {
				StreamPlayer.UnitDb = -10;
				StreamPlayer.UnitSize = 28;
				StreamPlayer.MaxDb = -10;
				break;
			}

			case SfxCatagory.RELOAD: {
				StreamPlayer.UnitDb = -2;
				StreamPlayer.UnitSize = 40;
				StreamPlayer.MaxDb = -2;
				break;
			}

			case SfxCatagory.BULLET_HIT: {
				StreamPlayer.UnitDb = -5;
				StreamPlayer.UnitSize = 28;
				StreamPlayer.MaxDb = -5;
				break;
			}

			case SfxCatagory.FLESH_HIT: {
				StreamPlayer.UnitDb = 6;
				StreamPlayer.UnitSize = 30;
				StreamPlayer.MaxDb = 6;
				StreamPlayer.Bus = "Master";
				break;
			}

			case SfxCatagory.PISTOL_FIRE: {
				StreamPlayer.UnitDb = 1;
				StreamPlayer.UnitSize = 60;
				StreamPlayer.MaxDb = 1;
				break;
			}

			case SfxCatagory.AK_FIRE: {
				StreamPlayer.UnitDb = 1;
				StreamPlayer.UnitSize = 60;
				StreamPlayer.MaxDb = 1;
				break;
			}

			case SfxCatagory.SHOTGUN_FIRE: {
				StreamPlayer.UnitDb = 4;
				StreamPlayer.UnitSize = 60;
				StreamPlayer.MaxDb = 4;
				break;
			}

			case SfxCatagory.CASING_TINK: {
				StreamPlayer.UnitDb = -4;
				StreamPlayer.UnitSize = 20;
				StreamPlayer.MaxDb = -4;
				break;
			}

			case SfxCatagory.DRIP: {
				StreamPlayer.UnitDb = -15;
				StreamPlayer.UnitSize = 25;
				break;
			}

			case SfxCatagory.CONCRETE_FOOTSTEPS: {
				StreamPlayer.UnitDb = 11;
				StreamPlayer.MaxDb = 11;
				StreamPlayer.UnitSize = 25;
				break;
			}

			case SfxCatagory.LEAVES_FOOTSTEPS: {
				StreamPlayer.UnitDb = 4;
				StreamPlayer.MaxDb = 4;
				StreamPlayer.UnitSize = 25;
				break;
			}

			case SfxCatagory.METAL_FOOTSTEPS: {
				StreamPlayer.UnitDb = 4;
				StreamPlayer.MaxDb = 4;
				StreamPlayer.UnitSize = 25;
				break;
			}

			case SfxCatagory.MARBLE_FOOTSTEPS: {
				StreamPlayer.UnitDb = -3;
				StreamPlayer.MaxDb = -3;
				StreamPlayer.UnitSize = 25;
				break;
			}
		}

		StreamPlayer.UnitDb -= Muffle;
		StreamPlayer.MaxDb -= Muffle;

		Game.RuntimeRoot.AddChild(StreamPlayer);
		StreamPlayer.Translation = Pos;
		StreamPlayer.Play();
	}
}
